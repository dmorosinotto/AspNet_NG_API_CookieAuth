using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AspNet.NGAPI.Services;

namespace AspNet.NGAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*services.AddControllersWithViews(); //TODO: SE MI SERVE SOLO API E NON ErrorPage POSSO USARE services.AddControllers(); 
            */
            services.AddControllers();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist"; //TODO: CONFIGURARE QUI LA PATH DI OUTPUT IN CUI COMPILA ng build --prod
            });

            //CONFIGURAZIONE COOKIE PER SAMESITE=None NEL CASO Cookies TRA SITI DIVERSI (OLTRE A CORS)
            services.ConfigureNonBreakingSameSiteCookies(); //Codice estenzione nel file SameSiteNoneFix.cs

            // CONFIGURO AUTH CON Cookies
            services.AddAuthentication("ng-cookies" /*CookieAuthenticationDefaults.AuthenticationScheme*/)
                    .AddCookie("ng-cookies" /*CookieAuthenticationDefaults.AuthenticationScheme*/
                    , options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.None; //IMPORTANTE SETTO SameSite=None NEL COSO DI SITI DIVERSI (OLTRE ALCORS)
                        //options.Cookie.SameSite = SameSiteMode.Strict; //SETTO SameSite=Strict NEL CASO DI API E SITO SULLO STESSO HOST + SICURO!
                        options.ReturnUrlParameter = "returnTo";
                        options.LoginPath = new PathString("/login");  //SETTO PAGINA DI LOGIN VERSO ANGULAR
                        options.Events = new CookieAuthenticationEvents()
                        {
                            OnRedirectToLogin = redirectContext =>
                            {
                                var uri = redirectContext.RedirectUri;
                                var isApi = redirectContext.Request.Headers["Accept"].ToString().Contains("application/json"); //CONTROLLO SE E' UNA CHIAMTA API
                                UriHelper.FromAbsolute(uri, out var scheme, out var host, out var path, out var query, out var fragment);
                                uri = UriHelper.BuildAbsolute(scheme, host, fragment: new FragmentString("#" + path.Value + query.Value));
                                System.Console.WriteLine($"{redirectContext.Request.Path} - {redirectContext.Request.GetEncodedUrl() } - Return URI=\t{uri}");
                                System.Console.WriteLine("Accept=\t" + redirectContext.Request.Headers["Accept"].ToString());
                                if (isApi)
                                {

                                    redirectContext.Response.Headers["Location"] = uri;
                                    redirectContext.Response.StatusCode = 401;
                                }
                                else
                                {
                                    redirectContext.Response.Redirect(uri);
                                }
                                return System.Threading.Tasks.Task.CompletedTask;
                            }
                        };
                        options.SlidingExpiration = true;
                        options.ExpireTimeSpan = new System.TimeSpan(0, 5, 0); //5min;
                    });
            // CONFIGURO EVENTUALI POLICY
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsUser", policy => policy.RequireRole("User"));
            });
            // REGISTRO SERVIZI PER AUTH
            services.AddSingleton<ICredentialService, FAKECredentialService>();

            // ABILITO CORS PER POTER USARE LE API DAI SITI "CLIENT" DA DOVE ARRIVANO LE CHIAMATE
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedSites", policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials(); //QUESTO E' NECESSARIO SE SI VUOLE ACCETTARE Cookie VIA CORS
                    policy.WithOrigins("https://localhost:5001", "https://localhost:5555",
                        "http://localhost:5000", "http://localhost:4200",
                        "http://localhost:8080", "http://192.168.77.201:8080", //prove con http-server -> dist
                        "https://192.168.77.201:8443", "https://127.0.0.1:8443"); //+SSL http-server -S -p 8443
                });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                /*app.UseExceptionHandler("/Error"); //TODO: POSSO RIMUOVERE ErrorPage SE FACCIO SOLO API IN C# 
                */
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                // QUANDO SONO IN PRODUZIONE L'APPLICAZIONE ANGULAR VIENE SERVITA DAI FILE STATICI PRODOTTI DALLA COMPILAZIONE ng build --prod
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            //ABILITO CORS PER LA LISTA DEI SITI SPECIFICATI
            app.UseCors("AllowedSites");

            // SE VOGLIO IMPORRE COOKIE SAMESITE Strict PER AVERE IL MASSIMO DI SICUREZZA (VA BENE SOLO SE API + NG SONO SU STESSO HOST)
            //app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Strict });
            // SE DEVO METTERE API E SITO NG SU HOST DIVERSI (OLTRE AD ABILITARE CORS) DEVO METTERE POLICY SameSite.None
            app.UseCookiePolicy();

            //AGGIUNGO GESTIONE AUTH
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                /*endpoints.MapControllerRoute( //TODO: SE NON USO ErrorPage QUESTO POSSO ANCHE ELIMINARLO E USO endpoints.MapControllers();
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                */
                endpoints.MapControllers();
                // TODO: CAPIRE SE EVENTUALMENTE SERVE FALLBACK PER SALTARE ALLA PAGINA STATICA DI index.html PER USARE ANGULAR + HTML5 Routing...
                // endpoints.MapFallbackToFile("wwwroot/dist/index.html");
                // endpoints.MapSpaFallbackRoute(
                //     name: "spa-fallback",
                //     defaults: new { controller = "Home", action = "Index" });
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                //spa.Options.SourcePath = "ClientApp"; //TODO: CAPIRE SE SPOSTO CARTELLA ClientApp SE DEVO AGGIORNARE (SERVE SE USO spa.UseAngularCliServer(npmScript: "start") X I PUNTAMENTI AI SORGENTI PER TROVARE DOVE LANCIARE SCRIPT package.json) 

                //spa.Options.DefaultPage = default "/index.html";
                //spa.Options.DefaultPageStaticFileOptions = null; //CONFIGURO StaticFile PER SERVIRE SPA SE SETTO =null -> USA default WebRootPath = LEGGE FILE STATICI DA /wwwroot

                if (env.IsDevelopment())
                {
                    /*spa.UseAngularCliServer(npmScript: "start"); //CON QUESTA CONFIGURAZIONE FA PARTIRE IN AUTOMATICO UN npm start E FA PROXY VERO localhost:4200 PERO' HO PROBLEMA CHE OGNI VOLTA RICOMPILA C# FA ANCHE RIPARTIRE COMPILAZIONE ng serve
                    */
                    // QUESTA CONFIGURA ESPLICITA NECESSITA CHE IO FACCIA PARTIRE A MANO ng serve MA RENDE INDIPENDENTE COMPILAZIONE C# DA QUELLA DI ng + POSSO USARE DOCKER CONTAINER PER GESTIRE node_modules + ANGULAR ESPONENDO localhost:4200
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }

    }
}
