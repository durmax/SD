using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class IndexBase : ComponentBase
    {
        //[Inject]
        //public AuthenticationStateProvider AuthenticationStateProvider { set; get; }
        [Inject]
        protected LangCodeService langCodeService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }


        private string fLang;
        private string tLang;
        protected string FLangCode
        {
            get { return fLang; }
            set {
                fLang = value;
                LocalStorageService.SetItemAsync("FLang", FLangCode);
            }
        }
        protected string TLangCode
        {
            get { return tLang; }
            set
            {
                tLang = value;
                LocalStorageService.SetItemAsync("TLang", TLangCode);
            }
        }

        protected string Word { get; set; }
        protected void Reverse()
        {
            string l = FLangCode;
            FLangCode = TLangCode;
            TLangCode = l;
        }

        protected override async Task OnInitializedAsync()
        {
            FLangCode = await LocalStorageService.GetItemAsync<string>("FLang");
            TLangCode = await LocalStorageService.GetItemAsync<string>("TLang");

            if (string.IsNullOrWhiteSpace(FLangCode))
                FLangCode = "de";

            if (string.IsNullOrWhiteSpace(TLangCode))
                TLangCode = "en";

            //TLang= string.IsNullOrWhiteSpace(TLang) ? await LocalStorageService.GetItemAsync<string>("TLang") :"en";
        }


    }
}
