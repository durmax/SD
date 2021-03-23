using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SD.Client.Services;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class OtherPagesBase : ComponentBase
    {
        [Inject]
        protected OtherPageService OtherPageService { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        protected IEnumerable<OtherPageResModel> otherPageModels;
        protected IEnumerable<OtherPageResModel> opRes;
        protected string opFilterVal = "Dict";

        protected string FavSite { get; private set; }
        protected string FavSiteOpc { get; set; } = "0.3";

        [Parameter]
        public bool Collapsed { get; set; } = true;    // hide by default
        [Parameter]
        public bool CanSetFavSite { get; set; }

        [Parameter]
        public string Word { get; set; }

        private string fLang;
        private string tLang;

        [Parameter]
        public string FLangCode //{ get; set; }
        {
            get { return fLang; }
            set
            {
                if (fLang != value)
                {
                    langChanged = true;
                    fLang = value;
                }
            }
        }
        [Parameter]
        public string TLangCode // { get; set; }
        {
            get { return tLang; }
            set
            {
                if (tLang != value)
                {
                    langChanged = true;
                    tLang = value;
                }
            }
        }

        public bool langChanged { get; set; } = false;
        public string Info { get; private set; }

        protected async Task GetP()
        {
            Collapsed = Collapsed ? false : true;
            //if (opRes == null)
            //{
            await GetOpRes();
            //}
        }

        protected async Task GetOpRes()
        {
            if (!Collapsed && opRes == null)
            {
                if (langChanged || otherPageModels == null)
                {
                    otherPageModels = null;
                    opRes = null;

                    var OPStr = await LocalStorageService.GetItemAsync<string>(FLangCode + "-" + TLangCode);
                    if (!string.IsNullOrEmpty(OPStr) && OPStr != "null")
                    {
                        otherPageModels = JsonConvert.DeserializeObject<IEnumerable<OtherPageResModel>>(OPStr);
                    }
                    else
                    {
                        otherPageModels = await OtherPageService.GetOPResModels(FLangCode, TLangCode);
                        if (FLangCode != TLangCode)
                        {
                            await LocalStorageService.SetItemAsync(FLangCode + "-" + TLangCode, otherPageModels);
                        }
                    }

                    opRes = OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
                    langChanged = false;
                }
                else
                {
                    if (otherPageModels != null)
                    {
                        opRes = null;
                        opRes = OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
                    }
                }
            }
        }

        protected async Task Favorite(OtherPageResModel otherPage)
        {
            if (CanSetFavSite || string.IsNullOrWhiteSpace(FavSite))
            {
                await LocalStorageService.SetItemAsync("fav" + "-" + FLangCode + "-" + TLangCode, otherPage.Pattern);
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + FLangCode + "-" + TLangCode);
            }
            else
            {
                Info = "/Languages";
            }
        }
        protected override async Task OnParametersSetAsync()
        {
            if (!Collapsed)
            {
                opRes = null;
                await GetOpRes();
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + FLangCode + "-" + TLangCode);
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(FavSite))
                FavSite = await LocalStorageService.GetItemAsync<string>("fav" + "-" + FLangCode + "-" + TLangCode);
        }
    }
}
