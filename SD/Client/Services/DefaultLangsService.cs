
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
            DefaultWordLang = await localStorageService.GetItemAsync<string>("FLang");
            DefaultToLang = await localStorageService.GetItemAsync<string>("TLang");
        }
    }
}
