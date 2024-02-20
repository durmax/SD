
using Blazored.LocalStorage;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class DefaultLangsService
    {
        private readonly ILocalStorageService localStorageService;
        public string DefaultWordLang { get; set; }
        public string DefaultToLang { get; set; }

        public DefaultLangsService(ILocalStorageService localStorageService)
        {
            this.localStorageService = localStorageService;
        }
        public async Task SetDefLangsAsync()
        {
            string fl = await localStorageService.GetItemAsync<string>("FLang");
            DefaultWordLang = (string.IsNullOrEmpty(fl) || fl == "null") ? "en" : fl;

            string tl = await localStorageService.GetItemAsync<string>("TLang");
            DefaultToLang = (string.IsNullOrEmpty(tl) || tl == "null") ? "de" : tl;
        }
    }
}
