using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class DefaultLangsService
    {
        public string DefaultWordLang { get; set; }
        public string DefaultToLang { get; set; }
        public LocalStorageAccessor LocalStorageAccessor { get; }

        public DefaultLangsService(LocalStorageAccessor localStorageAccessor)
        {
            LocalStorageAccessor = localStorageAccessor;
        }
        public async Task SetDefLangsAsync()
        {
            string fl = await LocalStorageAccessor.GetValueAsync<string>("FLang");
            DefaultWordLang = (string.IsNullOrEmpty(fl) || fl == "null") ? "en" : fl;

            string tl = await LocalStorageAccessor.GetValueAsync<string>("TLang");
            DefaultToLang = (string.IsNullOrEmpty(tl) || tl == "null") ? "de" : tl;
        }
    }
}
