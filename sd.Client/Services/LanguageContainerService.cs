using AKSoftware.Localization.MultiLanguages;

namespace sd.Client.Services
{
    public class LanguageContainerService(ILanguageContainerService languageContainer)
    {
        public string GetValue(string key)
        {
            if (languageContainer?.Keys == null)
            {
                return key;
            }

            return languageContainer.Keys[key] ?? key;
        }
    }
}
