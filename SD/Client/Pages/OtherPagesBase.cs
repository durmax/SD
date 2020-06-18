using Microsoft.AspNetCore.Components;
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

        protected IEnumerable<OtherPageResModel> otherPageModels;
        protected IEnumerable<OtherPageResModel> opRes;
        protected string opFilterVal = "Dict";

        [Parameter]
        public bool Collapsed { get; set; } = true;    // hide by default

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
        protected async Task GetP()
        {
            Collapsed = (Collapsed) ? false : true;
            if (opRes == null)
            {
                await GetOpRes();
            }
        }

        protected async Task GetOpRes()
        {
            if (!Collapsed)
            {
                if (langChanged || otherPageModels == null)
                {
                    otherPageModels = null;
                    opRes = null;

                    otherPageModels = await OtherPageService.GetOPResModels(FLangCode, TLangCode);

                    opRes =  OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
                    langChanged = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Word) && otherPageModels != null)
                    {
                        opRes = null;
                        opRes =  OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
                    }
                }
            }
        }
        protected override async Task OnParametersSetAsync()
        { 
            opRes = null;
            await GetOpRes();
        }

    }
}
