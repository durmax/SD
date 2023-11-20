using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

        protected List<OtherPageResModel> otherPageModels { get; set; }
        protected IEnumerable<OtherPageResModel> opRes { get; set; }

        protected string FavSite { get; private set; }

        [Parameter]
        public bool Collapsed { get; set; } = true;    // hide by default
        [Parameter]
        public bool CanSetFavSite { get; set; }

        [Parameter]
        public string Word { get; set; }

        [Parameter]
        public string MaxHeight { get; set; }

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

        double oldScreenY = 0;
        OtherPageResModel dragedOtherPage;
        protected async Task HandleDragStart(DragEventArgs e)
        {
            oldScreenY = e.ScreenY;
            dragedOtherPage = otherPageModels.Find(p => p.Eval == e.Button);
        }

        protected async Task Drop(DragEventArgs e)
        {
            await OtherPagesSortByEval(dragedOtherPage, e.ScreenY);
        }
        protected async Task OtherPagesSortByEval(OtherPageResModel otherPage, double newScreenY)
        {
            var x = (int)(oldScreenY - newScreenY) / 25;
            otherPageModels.Find(p => p.Host == otherPage.Host).Eval = otherPage.Eval - x;

            otherPageModels.Sort((x, y) => x.Eval.CompareTo(y.Eval));

            int i = -1;
            foreach (var oPage in otherPageModels)
            {
                i++;
                oPage.Eval = i;
            }
            await LocalStorageService.SetItemAsync(FLangCode + "-" + TLangCode, otherPageModels);
            opRes = null;
            opRes = OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
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
                        otherPageModels = JsonConvert.DeserializeObject<List<OtherPageResModel>>(OPStr);
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
