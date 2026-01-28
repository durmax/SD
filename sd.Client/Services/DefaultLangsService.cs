using sd.Client.Helpers;
using System.Threading.Tasks;

namespace sd.Client.Services
{
    public class DefaultLangsService
    {
        public string DefaultWordLang { get; set; }
        public string DefaultToLang { get; set; }
        public string FavSite { get; set; }

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

        public async Task<string> GetFavLinkAsync(string fl, string tl)
        {
            fl = string.IsNullOrWhiteSpace(fl) ? (DefaultWordLang ?? "en") : fl.Trim();
            tl = string.IsNullOrWhiteSpace(tl) ? (DefaultToLang ?? "de") : tl.Trim();

            return await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FavoriteSite(fl, tl));
        }

        public async Task SetFavorite(string fl, string tl, string pattern)
        {
                await LocalStorageAccessor.SetValueAsync(LangStorageKeys.FavoriteSite(fl, tl), pattern);
                FavSite = pattern;
        }
    }
}
