using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class OtherPagesBase : ComponentBase
    {
        [Inject]
        LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject]
        LoggingService logger { get; set; }
        [Inject]
        protected OtherPageService OtherPageService { get; set; }
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
        public string FLangCode
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
        public string TLangCode
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
            await OtherPagesChangeEval(dragedOtherPage, e.ScreenY);
        }
        protected async Task OtherPagesChangeEval(OtherPageResModel otherPage, double newScreenY)
        {
            var x = (int)(oldScreenY - newScreenY) / 25;
            otherPageModels.Find(p => p.Host == otherPage.Host).Eval = otherPage.Eval - x;

            await OtherPagesSort();
        }

        protected async Task OtherPagesSort()
        {
            otherPageModels.Sort((x, y) => x.Eval.CompareTo(y.Eval));

            int i = 0;
            foreach (var oPage in otherPageModels)
            {
                i++;
                oPage.Eval = i;
            }

            //var serializedOtherPageModels = JsonConvert.SerializeObject(otherPageModels);
            var serializedOtherPageModels = await LocalStorageAccessor.ToString(otherPageModels);

            await LocalStorageAccessor.SetValueAsync($"{FLangCode}{TLangCode}", serializedOtherPageModels);

            opRes = null;
            opRes = OtherPageService.MakeLinks(otherPageModels, Word, FLangCode, TLangCode);
        }

        protected async Task SetEvalAsync(ChangeEventArgs e, OtherPageResModel otherPage)
        {
            var oldEVal = otherPage.Eval;
            var newEVal = Int32.Parse(e.Value.ToString());

            if (oldEVal > newEVal)
            {
                foreach (var p in otherPageModels)
                {
                    if (p.Eval >= newEVal)
                    {
                        p.Eval++;
                    }
                }
            }
            else
            {
                foreach (var p in otherPageModels)
                {
                    if (p.Eval >= oldEVal)
                    {
                        p.Eval--;
                    }
                }
            }
            otherPageModels.Find(p => p.Host == otherPage.Host).Eval = newEVal;

            await OtherPagesSort();
        }

        protected async Task GetOpRes()
        {
            if (!Collapsed && opRes == null)
            {
                if (langChanged || otherPageModels == null)
                {
                    otherPageModels = null;
                    opRes = null;

                    logger.Log(this.ToString(), LogLevel.Information, "Before GetItemAsync");
                    string OPStr = await LocalStorageAccessor.GetValueAsync<string>($"{FLangCode}{TLangCode}");
                    logger.Log(this.ToString(), LogLevel.Information, "After GetItemAsync " + $"{FLangCode}{TLangCode}");
                    logger.Log(this.ToString(), LogLevel.Information, OPStr);

                    if (!string.IsNullOrEmpty(OPStr) && OPStr != "null")
                    {
                        otherPageModels = JsonConvert.DeserializeObject<List<OtherPageResModel>>(OPStr);
                    }
                    else
                    {
                        logger.Log(this.ToString(), LogLevel.Information, "after else ------ ");
                        otherPageModels = await OtherPageService.GetOPResModels(FLangCode, TLangCode);
                        if (FLangCode != TLangCode)
                        {
                            try
                            {
                                //var serializedOtherPageModels = JsonConvert.SerializeObject(otherPageModels);

                                 var serializedOtherPageModels = await LocalStorageAccessor.ToString(otherPageModels);
                                await LocalStorageAccessor.SetValueAsync($"{FLangCode}{TLangCode}", serializedOtherPageModels);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(this.ToString(), LogLevel.Error, "LocalStorageAccessor.SetValueAsync " + $"{FLangCode}{TLangCode} " + ex.Message);
                                throw;
                            }

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
                try
                {
                    await GetOpRes();
                    FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{FLangCode}{TLangCode}");
                }
                catch (Exception ex)
                {
                    logger.Log(this.ToString(), LogLevel.Error, "GetOpRes " + ex.Message);
                    throw;
                }
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(FavSite))
                FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{FLangCode}{TLangCode}");
        }
    }
}
