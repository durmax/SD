using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using SD.Client.Models;
using Microsoft.JSInterop;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {

        [Inject]
        protected LangCodeService LangCodeService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        [Inject]
        IJSRuntime JSRuntime { get; set; }

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


        protected string Word { get; set; }
        protected ElementReference wordRef;

        protected void Reverse()
        {
            LangCode l = SelectedFL;
            SelectedFL = SelectedTL;
            SelectedTL = l;
        }


        [Parameter] public List<LangCode> LangCodes { get; set; }
        protected async Task<IEnumerable<LangCode>> SearchLangs(string searchText)
        {
            return await Task.FromResult(LangCodes.Where(x => x.Value.ToLower().Contains(searchText.ToLower())).ToList());
        }

        protected override async Task OnInitializedAsync()
        {

            string fl = await LocalStorageService.GetItemAsync<string>("FLang");
            fl = (string.IsNullOrWhiteSpace(fl) || fl == "null") ? "en" : fl;
            string tl = await LocalStorageService.GetItemAsync<string>("TLang");
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

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
               // await JSRuntime.InvokeVoidAsync("focusElement", wordRef);
                await JSRuntime.InvokeVoidAsync(
    "exampleJsFunctions.focusElement", "wordId");
            }
        }
    }
}
