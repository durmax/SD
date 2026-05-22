namespace sd.Client.Helpers;

public static class LangStorageKeys
{
    public const string FromLang = "FLang";   // mother
    public const string ToLang = "TLang";     // learn
    public const string UiLang = "UILang";
    public const string KnownLangs = "Langs";
    public const string LanguageSettings = "LanguageSettings";

    public static string FavoriteSite(string from, string to) => $"fav-{from}{to}";
    public static string DictionaryOrder(string from, string to) => $"{from}{to}";
}
