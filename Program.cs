using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNet.NGAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //PROVA CON DIVERSI URL BINDING ASSOCIATI AL SITO - SI PUO' FARE ANCHE DA RIGA DI COMANDO dotnet run --urls 'https://localhost:5001;https://localhost:5555'
                    webBuilder.UseUrls("http://localhost:5000;https://localhost:5001;https://localhost:5555"); //VERIFICATO CHE FUNZIONA Cookie OGNUNO PER IL SUO DOMINIO!
                });
    }
}
