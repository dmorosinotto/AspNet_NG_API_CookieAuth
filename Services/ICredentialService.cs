using System.Threading.Tasks;

namespace AspNet.NGAPI.Services
{
    public interface ICredentialService
    {
        Task<string> validateAsync(string username, string password); //RITORNA RUOLO OPPURE null SE NON VALIDO
    }

    public class FAKECredentialService : ICredentialService
    {
        //FAKE CREDENTIAL SERVICE IMPLEMENTATION...
        public async Task<string> validateAsync(string username, string password)
        {
            //TODO: QUI NELL'IMPLEMENTAZIONE REALE DOVRO USARE HttpClient PER CHIAMARE API ESPOSTA DA DAVIDE PER VALIDARE CREDENZIALI E RICAVARE Ruoli/UserProfile

            if (username != password) return null;
            switch (username.ToLower())
            {
                case "xxx": return "Admin";
                default: return "User";
            }
        }
    }
}