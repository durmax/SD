using System.Threading.Tasks;

namespace sd.Client.Services;

public class UserPreferencesService(LocalStorageAccessor localStorage)
{
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
