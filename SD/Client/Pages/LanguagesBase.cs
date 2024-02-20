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
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        protected DefaultLangsService DefaultLangsService { get; set; }

        [Inject]
        KnownLangsService KnownLangsService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }
        [Inject]
        public ILanguageContainerService LanguageContainer { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }

        protected List<string> KnownLangs { get; set; }

        private LangCode SFL;
        private LangCode STL;
        private LangCode LToAdd;

        protected string Fl { get; set; }
        protected string Tl { get; set; }


        protected async Task ResetOPAsync()
        {
            await LocalStorageService.RemoveItemAsync(Fl + "-" + Tl);
            SetLangsStr();
            NavigationManager.NavigateTo("Languages", true);
        }

        protected void Reverse()
        {
            (Tl, Fl) = (Fl, Tl);
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
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("ar-SY"));
                        LocalStorageService.SetItemAsync("UILang", "ar-SY");
                        break;
                    case "de":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
                        LocalStorageService.SetItemAsync("UILang", "de-DE");
                        break;
                    case "fr":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                        LocalStorageService.SetItemAsync("UILang", "fr-FR");
                        break;
                    case "es":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
                        LocalStorageService.SetItemAsync("UILang", "es-ES");
                        break;
                    case "fa":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
                        LocalStorageService.SetItemAsync("UILang", "fa-IR");
                        break;
                    case "it":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
                        LocalStorageService.SetItemAsync("UILang", "it-IT");
                        break;
                    case "pt":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));
                        LocalStorageService.SetItemAsync("UILang", "pt-PT");
                        break;
                    case "ru":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                        LocalStorageService.SetItemAsync("UILang", "ru-RU");
                        break;
                    case "tr":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
                        LocalStorageService.SetItemAsync("UILang", "tr-TR");
                        break;
                    default:
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                        LocalStorageService.SetItemAsync("UILang", "en-US");
                        break;
                }
            }
            catch
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
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
            // Tl = "en";
            // Fl = "de";
            //try
            //{
                Fl = DefaultLangsService.DefaultWordLang;
                Tl = DefaultLangsService.DefaultToLang;
            //}
            //catch
            //{
            //}
            //Fl = (string.IsNullOrEmpty(Fl) || Fl == "null") ? "en" : Fl;
            //Tl = (string.IsNullOrEmpty(Tl) || Tl == "null") ? "de" : Tl;

            SelectedFL = new LangCode
            {
                Key = Fl,
                Value = LangCodeService.Langs[Fl]
            };

            SelectedTL = new LangCode
            {
                Key = Tl,
                Value = LangCodeService.Langs[Tl]
            };

            KnownLangsService.LangsStr = await LocalStorageService.GetItemAsync<string>("Langs");
            BuildKnownLangs();

            LangCodes = new List<LangCode>();
            foreach (var item in LangCodeService.Langs)
            {
                LangCode langCode = new()
                {
                    Key = item.Key,
                    Value = item.Value
                };

                LangCodes.Add(langCode);
            }
        }
    }
}