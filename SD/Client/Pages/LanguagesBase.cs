using AKSoftware.Localization.MultiLanguages;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SD.Client.Models;
using SD.Client.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class LanguagesBase : ComponentBase
    {
        [Inject]
        public UserService UserService { set; get; }

        [Inject]
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        protected DefaultLangsService DefaultLangsService { get; set; }

        [Inject]
        KnownLangsService KnownLangsService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }
        [Inject]
        public ILanguageContainerService languageContainer { get; set; }

        protected List<string> KnownLangs { get; set; }

        private LangCode SFL;
        private LangCode STL;
        private LangCode LToAdd;

        protected string fl { get; set; }
        protected string tl { get; set; }


        protected void Reverse()
        {
            string l = fl;
            fl = tl;
            tl = l;
        }

        protected LangCode SelectedFL
        {
            get { return SFL; }
            set
            {
                SFL = value;
                LocalStorageService.SetItemAsync("FLang", SelectedFL.Key);
                KnownLangsService.AddKnownLang(SelectedFL.Key);
                DefaultLangsService.DefaultWordLang = SelectedFL.Key;
            }
        }

        protected LangCode SelectedTL
        {
            get { return STL; }
            set
            {
                STL = value;
                LocalStorageService.SetItemAsync("TLang", SelectedTL.Key);
                SetUILang(SelectedTL.Key);
                KnownLangsService.AddKnownLang(SelectedTL.Key);
                DefaultLangsService.DefaultToLang = SelectedTL.Key;
            }
        }

        protected LangCode LangToAdd
        {
            get { return LToAdd; }
            set
            {
                LToAdd = value;
                KnownLangsService.LangsStr += "," + LToAdd.Key;
                BuildKnownLangs();
            }
        }

        private void SetUILang(string langCode)
        {
            try
            {
                switch (langCode)
                {
                    case "ar":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("ar-SY"));
                        LocalStorageService.SetItemAsync("UILang", "ar-SY");
                        break;
                    case "de":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
                        LocalStorageService.SetItemAsync("UILang", "de-DE");
                        break;
                    case "fr":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                        LocalStorageService.SetItemAsync("UILang", "fr-FR");
                        break;
                    case "es":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
                        LocalStorageService.SetItemAsync("UILang", "es-ES");
                        break;
                    case "fa":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
                        LocalStorageService.SetItemAsync("UILang", "fa-IR");
                        break;
                    case "it":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
                        LocalStorageService.SetItemAsync("UILang", "it-IT");
                        break;
                    case "pt":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));
                        LocalStorageService.SetItemAsync("UILang", "pt-PT");
                        break;
                    case "ru":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                        LocalStorageService.SetItemAsync("UILang", "ru-RU");
                        break;
                    case "tr":
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
                        LocalStorageService.SetItemAsync("UILang", "tr-TR");
                        break;
                    default:
                        languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                        LocalStorageService.SetItemAsync("UILang", "en-US");
                        break;
                }
            }
            catch
            {
                languageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                LocalStorageService.SetItemAsync("UILang", "en-US");
            }
        }



        [Parameter] public List<LangCode> LangCodes { get; set; }
        protected async Task<IEnumerable<LangCode>> SearchLangs(string searchText)
        {
            return await Task.FromResult(LangCodes.Where(x => x.Value.ToLower().Contains(searchText.ToLower())).ToList());
        }



        protected void RemoveKnownLang(string lang)
        {
            if (lang != SelectedFL.Key && lang != SelectedTL.Key)
            {
                KnownLangs.Remove(lang);
                SetLangsStr();
            }
        }

        protected void BuildKnownLangs()
        {
            KnownLangsService.AddKnownLang(SelectedFL.Key);
            KnownLangsService.AddKnownLang(SelectedTL.Key);

            KnownLangs = new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;

            SetLangsStr();
        }

        private void SetLangsStr()
        {
            KnownLangsService.LangsStr = null;
            foreach (var item in KnownLangs)
            {
                KnownLangsService.LangsStr += "," + item;
            }
            LocalStorageService.SetItemAsync("Langs", KnownLangsService.LangsStr);
        }

        protected override async Task OnInitializedAsync()
        {
             tl = "en";
             fl = "de";
            try
            {
                fl = await LocalStorageService.GetItemAsync<string>("FLang");
                tl = await LocalStorageService.GetItemAsync<string>("TLang");
            }
            catch
            {
            }
            fl = (string.IsNullOrWhiteSpace(fl) || fl == "null") ? "en" : fl;
            tl = (string.IsNullOrWhiteSpace(tl) || tl == "null") ? "de" : tl;

            SelectedFL = new LangCode
            {
                Key = fl,
                Value = LangCodeService.Langs[fl]
            };

            SelectedTL = new LangCode
            {
                Key = tl,
                Value = LangCodeService.Langs[tl]
            };

            KnownLangsService.LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");
            BuildKnownLangs();

            LangCodes = new List<LangCode>();
            foreach (var item in LangCodeService.Langs)
            {
                LangCode langCode = new LangCode
                {
                    Key = item.Key,
                    Value = item.Value
                };

                LangCodes.Add(langCode);
            }
        }
    }
}