using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AspNet.NGAPI.Models;
using AspNet.NGAPI.Services;

namespace AspNet.NGAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> logger;
        private readonly ICredentialService credentialService;

        public AuthController(ILogger<AuthController> logger, ICredentialService credentialService)
        {
            this.logger = logger;
            this.credentialService = credentialService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model, [FromQuery] string returnTo = "/") //ARRIVO QUI QUANDO FACCIO SUBMIT FORM DI LOGIN - uso CookieAuth A MANO CON UTENTE LOCALI
        {
            //CONTROLLO UTENTE LOCALI IN BASE utente + password HASh - VEDI CODICE INTERNO AL REPOSITORY!!
            var role = await credentialService.validateAsync(model.Username, model.Password);
            if (role == null)
                return Unauthorized(); //SE NON TROVO UTENTE TORNA 401 = Unauthorized

            //SE CREDENZIALI VALIDE ALLORA CREO CLAIMS -> Identity PER IL CookieSchema -> PRINCIPAL --> POI FINISCE IN HttpContext.User
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Username.Length.ToString()), //SID DELL'UTENTE
                new Claim(ClaimTypes.Name, model.Username),  //CLAIM DI DEFAULT PER IL NOME UTENTE
                new Claim(ClaimTypes.Role, role),  //CLAIM DI DEFAULT PER I RUOLI ()
                //new Claim(ClaimTypes.Role, "Speaker"),  //EVENTUALI ALTRI RUOLI VANNO AGGUNTI SEPARATAMENTE
                //new Claim("Permission", "AddConference"),
                //new Claim("FavoriteColor", user.FavoriteColor) //CLAIM TOTALMENTE CUSTOM DA LEGARE A UTENTE X POLICY
            };

            var identity = new ClaimsIdentity(claims, //QUI DEVO SPECIFICARE LO SCHEMA DELL'AUTH PER CUI VALGONO QUESTI CLAIMS
                "ng-cookies" /*CookieAuthenticationDefaults.AuthenticationScheme*/); //QUI LO EMETTO PER LO SCHEMA DI DEFAULT="Cookies"
            var principal = new ClaimsPrincipal(identity); //ALLA FINE CREO IL PRINCIPAL

            await HttpContext.SignInAsync( //QUESTA CHIAMATA ALLA FINE FINISCE IL SIGNIN CON CLAIMS PASSATE
                "ng-cookies"/*CookieAuthenticationDefaults.AuthenticationScheme*/, //IMPORTANTE RIPORTARE LO SCHEMA GIUSTO QUI USA DEFAULT="Cookies"
                principal,
                new AuthenticationProperties { IsPersistent = false }); //SETTA PROPRIETA' PERSISTENT VOLENDO SI PUO' SETTARE ANCHE DURATA ExpireUtc / Sliding / AllowRefresh

            //return LocalRedirect(returnTo); //IMPORTANTE USARE LocalRedirect PER EVITARE ATTACHI CHE PASSANO ReturnUrl a siti malevoli esterni (SI ASSICURA DI SALTARE A PAGINE INTERNE)
            return Ok(true);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout() //ARRIVO QUI QUANDO FACCIO LOGOUT E DEVO ELIMINARE IL Cookie
        {
            //ESEGUO IL LOGOUT (cancella il Cookie) 
            await HttpContext.SignOutAsync("ng-cookies");
            return Ok();
        }
    }
}
