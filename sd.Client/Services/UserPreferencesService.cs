using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class UserPreferencesService(LocalStorageAccessor localStorage)
{
    //public string FLang { get; set; }
    //public string TLang { get; set; }
    //public string Langs { get; set; }
    //public string UiLang { get; set; }

    //public async Task<string> GetFLang() =>  FLang ??= await localStorage.GetValueAsync<string>(LangStorageKeys.FromLang);
    //public async Task<string> GetTLang() => TLang ??= await localStorage.GetValueAsync<string>(LangStorageKeys.ToLang);
    //public async Task<string> GetLangs() => Langs ??= await localStorage.GetValueAsync<string>(LangStorageKeys.KnownLangs);
    //public async Task<string> GetUILang() => UiLang ??= await localStorage.GetValueAsync<string>(LangStorageKeys.UiLang);

    //public async Task SetFLang(string fLang) => await localStorage.SetValueAsync(LangStorageKeys.FromLang, fLang);
    //public async Task SetTLang(string tLang) => await localStorage.SetValueAsync(LangStorageKeys.ToLang, tLang);
    //public async Task SetLangs(string langs) => await localStorage.SetValueAsync(LangStorageKeys.KnownLangs, langs);
    //public async Task SetUILang(string uiLang) => await localStorage.SetValueAsync(LangStorageKeys.UiLang, uiLang);

    public LanguageSettings LanguageSettings { get; set; }

    public async Task<LanguageSettings> GetSettingsAsync(bool refresh)
    {
        if (refresh || LanguageSettings == null)
            LanguageSettings = await localStorage.GetValueAsync<LanguageSettings>("LangSettings")
                    ?? LanguageSettings.Default;

        return LanguageSettings;
    }

    public async Task SetSettingsAsync(LanguageSettings settings)
    {
        await localStorage.SetValueAsync("LangSettings", settings);
    }

}


//public class LanguageSettings
//{
//    public string? FromLang { get; set; }
//    public string? ToLang { get; set; }
//    public string? KnownLangs { get; set; }
//    public string? UiLang { get; set; }
//}

public record LanguageSettings(
    string FromLang,
    string ToLang,
    string[] KnownLangs,
    string UiLang
)
{
    public static LanguageSettings Default =>
        new("en", "de", new[] { "en", "de" }, "en");
}
