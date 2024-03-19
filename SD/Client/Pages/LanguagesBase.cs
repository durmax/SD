using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
        LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject]
        IJSRuntime JsRuntime { set; get; }
        [Inject]
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        protected DefaultLangsService DefaultLangsService { get; set; }

        [Inject]
        KnownLangsService KnownLangsService { get; set; }

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

        protected void Reverse()
        {
            (Tl, Fl) = (Fl, Tl);
        }

        protected LangCode SelectedFL
        {
            get { return SFL; }
            set
            {
                if (value != null)
                {
                    SFL = value;
                    LocalStorageAccessor.SetValueAsync("FLang", SelectedFL.Key);
                    KnownLangsService.AddKnownLang(SelectedFL.Key);
                    DefaultLangsService.DefaultWordLang = SelectedFL.Key;
                    LangToAdd = value;
                    Fl = value.Key;
                }
            }
        }

        protected LangCode SelectedTL
        {
            get { return STL; }
            set
            {
                if (value != null)
                {
                    STL = value;
                    LocalStorageAccessor.SetValueAsync("TLang", SelectedTL.Key);
                    SetUILang(SelectedTL.Key);
                    KnownLangsService.AddKnownLang(SelectedTL.Key);
                    DefaultLangsService.DefaultToLang = SelectedTL.Key;
                    LangToAdd = value;
                    Tl = value.Key;
                }
            }
        }

        protected LangCode LangToAdd
        {
            get { return LToAdd; }
            set
            {
                if (value != null)
                {
                    LToAdd = value;
                    KnownLangsService.LangsStr += "," + LToAdd.Key;
                    BuildKnownLangs();
                }
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
                        LocalStorageAccessor.SetValueAsync("UILang", "ar-SY");
                        break;
                    case "de":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
                        LocalStorageAccessor.SetValueAsync("UILang", "de-DE");
                        break;
                    case "fr":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
                        LocalStorageAccessor.SetValueAsync("UILang", "fr-FR");
                        break;
                    case "es":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
                        LocalStorageAccessor.SetValueAsync("UILang", "es-ES");
                        break;
                    case "fa":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("fa-IR"));
                        LocalStorageAccessor.SetValueAsync("UILang", "fa-IR");
                        break;
                    case "it":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
                        LocalStorageAccessor.SetValueAsync("UILang", "it-IT");
                        break;
                    case "pt":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));
                        LocalStorageAccessor.SetValueAsync("UILang", "pt-PT");
                        break;
                    case "ru":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                        LocalStorageAccessor.SetValueAsync("UILang", "ru-RU");
                        break;
                    case "tr":
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
                        LocalStorageAccessor.SetValueAsync("UILang", "tr-TR");
                        break;
                    default:
                        LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                        LocalStorageAccessor.SetValueAsync("UILang", "en-US");
                        break;
                }
            }
            catch
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                LocalStorageAccessor.SetValueAsync("UILang", "en-US");
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


        protected async Task ResetOPAsync()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"You try to reset {Fl}-{Tl}, are you sure?");
            if (confirmed)
            {
                await LocalStorageAccessor.RemoveAsync($"{Fl}{Tl}");
                SetLangsStr();
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }
        }

        protected async Task RemoveAllDataAsync()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "You try to delete all data, are you sure?");
            if (confirmed)
            {
                await LocalStorageAccessor.Clear();
                await JsRuntime.InvokeVoidAsync("alert", $"Your data are deleted");

                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }
        }

        protected void BuildKnownLangs()
        {
            KnownLangsService.AddKnownLang(SelectedFL?.Key);
            KnownLangsService.AddKnownLang(SelectedTL?.Key);

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
            LocalStorageAccessor.SetValueAsync("Langs", KnownLangsService.LangsStr);
        }

        protected override async Task OnInitializedAsync()
        {
            await DefaultLangsService.SetDefLangsAsync();

            Fl = DefaultLangsService.DefaultWordLang;
            Tl = DefaultLangsService.DefaultToLang;

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

            KnownLangsService.LangsStr = await LocalStorageAccessor.GetValueAsync<string>("Langs");
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