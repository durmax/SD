using sd.Client.Helpers;
using System.Threading.Tasks;

namespace sd.Client.Services
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
            string fl = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FromLang);
            DefaultWordLang = (string.IsNullOrEmpty(fl) || fl == "null") ? "en" : fl;

            string tl = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.ToLang);
            DefaultToLang = (string.IsNullOrEmpty(tl) || tl == "null") ? "de" : tl;
        }
    }
}
