using sd.Client.Helpers;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class UserPreferencesService(LocalStorageAccessor localStorage)
{
    private LanguageSettings? _languageSettings;

    public async Task<LanguageSettings> GetSettingsAsync(bool refresh = false)
    {
        if (refresh || _languageSettings is null)
        {
            var settings = await localStorage.GetValueAsync<LanguageSettings>(
                LangStorageKeys.LanguageSettings);

            _languageSettings = LanguageSettings.Normalize(settings);
        }

        return _languageSettings;
    }

    public async Task SetSettingsAsync(LanguageSettings settings)
    {
        _languageSettings = LanguageSettings.Normalize(settings);

        await localStorage.SetValueAsync(
            LangStorageKeys.LanguageSettings,
            _languageSettings);
    }
}

public record LanguageSettings(
    string? FromLang,
    string? ToLang,
    string[]? KnownLangs,
    string? UiLangCode
)
{
    public static LanguageSettings Default =>
        new("en", "de", new[] { "en", "de" }, "en");

    public static LanguageSettings Normalize(LanguageSettings? settings)
    {
        return new LanguageSettings(
            string.IsNullOrWhiteSpace(settings?.FromLang)
                ? Default.FromLang
                : settings.FromLang,

            string.IsNullOrWhiteSpace(settings?.ToLang)
                ? Default.ToLang
                : settings.ToLang,

            settings?.KnownLangs is null || settings.KnownLangs.Length == 0
                ? Default.KnownLangs
                : settings.KnownLangs,

            string.IsNullOrWhiteSpace(settings?.UiLangCode)
                ? Default.UiLangCode
                : settings.UiLangCode
        );
    }
}