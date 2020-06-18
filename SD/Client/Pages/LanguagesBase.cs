using Blazored.LocalStorage;
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
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        private LangCode SFL;
        private LangCode STL;

        protected LangCode SelectedFL
        {
            get { return SFL; }
            set
            {
                SFL = value;
                LocalStorageService.SetItemAsync("FLang", SelectedFL.Key);
            }
        }

        protected LangCode SelectedTL
        {
            get { return STL; }
            set
            {
                STL = value;
                LocalStorageService.SetItemAsync("TLang", SelectedTL.Key);
            }
        }

        [Parameter] public List<LangCode> LangCodes { get; set; }
        protected async Task<IEnumerable<LangCode>> SearchLangs(string searchText)
        {
            return await Task.FromResult(LangCodes.Where(x => x.Value.ToLower().Contains(searchText.ToLower())).ToList());
        }

        protected void Reverse()
        {
            LangCode l = SelectedFL;
            SelectedFL = SelectedTL;
            SelectedTL = l;
        }

        protected override async Task OnInitializedAsync()
        {
            string fl = "en";
            string tl = "de";
            try { 
             fl = await LocalStorageService.GetItemAsync<string>("FLang");
             tl = await LocalStorageService.GetItemAsync<string>("TLang");
            }
            catch { 
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
