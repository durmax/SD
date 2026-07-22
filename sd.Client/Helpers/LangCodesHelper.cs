using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Globalization;

namespace sd.Client.Helpers;

public static class LangCodesHelper
{
    public static IReadOnlyDictionary<string, string> Langs { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["af"] = "afrikaans",
            ["ar"] = "arabic",
            ["as"] = "assamese",
            ["av"] = "avaric",
            ["ay"] = "aymara",
            ["az"] = "azerbaijani",
            ["ba"] = "bashkir",
            ["be"] = "belarusian",
            ["bg"] = "bulgarian",
            ["bh"] = "bihari",
            ["bm"] = "bambara",
            ["bi"] = "bislama",
            ["bn"] = "bengali",
            ["bo"] = "tibetan",
            ["br"] = "breton",
            ["bs"] = "bosnian",
            ["ca"] = "catalan",
            ["ce"] = "chechen",
            ["ch"] = "chamorro",
            ["co"] = "corsican",
            ["cr"] = "cree",
            ["cs"] = "czech",
            ["cu"] = "church",
            ["cv"] = "chuvash",
            ["cy"] = "welsh",
            ["da"] = "danish",
            ["de"] = "german",
            ["dv"] = "divehi",
            ["dz"] = "dzongkha",
            ["ee"] = "ewe",
            ["el"] = "greek",
            ["en"] = "english",
            ["eo"] = "esperanto",
            ["es"] = "spanish",
            ["et"] = "estonian",
            ["eu"] = "basque",
            ["fa"] = "persian",
            ["ff"] = "fulah",
            ["fi"] = "finnish",
            ["fj"] = "fijian",
            ["fo"] = "faroese",
            ["fr"] = "french",
            ["fy"] = "western",
            ["ga"] = "irish",
            ["gd"] = "gaelic",
            ["gl"] = "galician",
            ["gn"] = "guarani",
            ["gu"] = "gujarati",
            ["gv"] = "manx",
            ["ha"] = "hausa",
            ["he"] = "hebrew",
            ["hi"] = "hindi",
            ["hr"] = "croatian",
            ["ht"] = "haitian",
            ["hu"] = "hungarian",
            ["hy"] = "armenian",
            ["hz"] = "herero",
            ["ia"] = "interlingua",
            ["id"] = "indonesian",
            ["ie"] = "interlingue",
            ["ig"] = "igbo",
            ["ii"] = "sichuanyi",
            ["ik"] = "inupiaq",
            ["io"] = "ido",
            ["is"] = "icelandic",
            ["it"] = "italian",
            ["iu"] = "inuktitut",
            ["ja"] = "japanese",
            ["jv"] = "javanese",
            ["ka"] = "georgian",
            ["kg"] = "kongo",
            ["ki"] = "kikuyu",
            ["kj"] = "kuanyama",
            ["kk"] = "kazakh",
            ["kl"] = "kalaallisut",
            ["kn"] = "kannada",
            ["ko"] = "korean",
            ["kr"] = "kanuri",
            ["ks"] = "kashmiri",
            ["ku"] = "kurdish",
            ["kv"] = "komi",
            ["kw"] = "cornish",
            ["ky"] = "kirghiz",
            ["la"] = "latin",
            ["lb"] = "luxembourgish",
            ["lg"] = "ganda",
            ["li"] = "limburgan",
            ["ln"] = "lingala",
            ["lo"] = "lao",
            ["lt"] = "lithuanian",
            ["lu"] = "luba-katanga",
            ["lv"] = "latvian",
            ["mg"] = "malagasy",
            ["mh"] = "marshallese",
            ["mi"] = "maori",
            ["mk"] = "macedonian",
            ["ml"] = "malayalam",
            ["mn"] = "mongolian",
            ["mr"] = "marathi",
            ["ms"] = "malay",
            ["mt"] = "maltese",
            ["my"] = "burmese",
            ["na"] = "nauru",
            ["nb"] = "bokmål",
            ["nd"] = "ndebele",
            ["ne"] = "nepali",
            ["ng"] = "ndonga",
            ["nl"] = "dutch",
            ["nn"] = "norwegian",
            ["no"] = "norwegian",
            ["nr"] = "ndebele",
            ["nv"] = "navajo",
            ["ny"] = "chichewa",
            ["oc"] = "occitan",
            ["oj"] = "ojibwa",
            ["om"] = "oromo",
            ["or"] = "oriya",
            ["os"] = "ossetian",
            ["pa"] = "panjabi",
            ["pi"] = "pali",
            ["pl"] = "polish",
            ["ps"] = "pushto",
            ["pt"] = "portuguese",
            ["qu"] = "quechua",
            ["rm"] = "romansh",
            ["rn"] = "rundi",
            ["ro"] = "romanian",
            ["ru"] = "russian",
            ["rw"] = "kinyarwanda",
            ["sa"] = "sanskrit",
            ["sc"] = "sardinian",
            ["sd"] = "sindhi",
            ["se"] = "northernsami",
            ["sg"] = "sango",
            ["si"] = "sinhala",
            ["sk"] = "slovak",
            ["sl"] = "slovenian",
            ["sm"] = "samoan",
            ["sn"] = "shona",
            ["so"] = "somali",
            ["sq"] = "albanian",
            ["sr"] = "serbian",
            ["ss"] = "swati",
            ["st"] = "sotho",
            ["su"] = "sundanese",
            ["sv"] = "swedish",
            ["sw"] = "swahili",
            ["ta"] = "tamil",
            ["te"] = "telugu",
            ["tg"] = "tajik",
            ["th"] = "thai",
            ["ti"] = "tigrinya",
            ["tk"] = "turkmen",
            ["tl"] = "tagalog",
            ["tn"] = "tswana",
            ["to"] = "tonga",
            ["tr"] = "turkish",
            ["ts"] = "tsonga",
            ["tt"] = "tatar",
            ["tw"] = "twi",
            ["ty"] = "tahitian",
            ["ug"] = "uighur",
            ["uk"] = "ukrainian",
            ["ur"] = "urdu",
            ["uz"] = "uzbek",
            ["ve"] = "venda",
            ["vi"] = "vietnamese",
            ["vo"] = "volapük",
            ["wa"] = "walloon",
            ["wo"] = "wolof",
            ["xh"] = "Xhosa",
            ["yi"] = "yiddish",
            ["yo"] = "yoruba",
            ["za"] = "zhuang",
            ["zh"] = "chinese",
            ["zu"] = "zulu",
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string> UiLangs { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ar"] = "arabic",
            ["en"] = "english",
            ["de"] = "german",
            ["fr"] = "french",
            ["ru"] = "russian",
            ["tr"] = "turkish",
            ["it"] = "italian",
            ["es"] = "spanish",
            ["pt"] = "brazilian",
            ["fa"] = "Farsi"

        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string> CultureCodes { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["arabic"] = "ar-SY",
            ["english"] = "en-US",
            ["german"] = "de-DE",
            ["french"] = "fr-FR",
            ["russian"] = "ru-RU",
            ["spanish"] = "es-ES",
            ["turkish"] = "tr-TR",
            ["italian"] = "it-IT",
            ["brazilian"] = "pt-BR",
            ["Farsi"] = "fa-IR"

        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static string GetLanguageNameOrEmpty(string code) =>
        Langs.TryGetValue(code, out var name) ? name : code;

    public static bool TryGetLanguageName(string code, out string? name) =>
        Langs.TryGetValue(code, out name);

    public static bool GetUiCulture(string uiLangCode, out CultureInfo? culture)
    {
        culture = null;

        var languageName = GetLanguageNameOrEmpty(uiLangCode);

        if (string.IsNullOrWhiteSpace(languageName))
            return false;

        if (!CultureCodes.TryGetValue(languageName, out var cultureCode))
            return false;

        culture = CultureInfo.GetCultureInfo(cultureCode);

        return true;
    }
}
