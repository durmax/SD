using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class OtherPagesBase : ComponentBase
    {
        [Inject] LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject] ILogger<OtherPagesBase> log { get; set; }
        [Inject] protected OtherPageService OtherPageService { get; set; }
        [Inject] protected ApiService ApiService { get; set; }
        [Parameter] public bool Collapsed { get; set; } = true;    // hide by default
        [Parameter] public bool Sortable { get; set; } = false;
        [Parameter] public bool CanSetFavSite { get; set; }
        [Parameter] public string Word { get; set; }
        [Parameter] public string MaxHeight { get; set; }

        protected List<OtherPageResModel> opRes { get; set; }
        protected string FavSite { get; private set; }
        public bool langChanged { get; set; } = false;
        public string Info { get; private set; }
        private string fLang;
        private string tLang;

        protected async Task SortListAsync(FluentSortableListEventArgs args)
        {
            if (args is null || args.OldIndex == args.NewIndex)
            {
                return;
            }

            var oldIndex = args.OldIndex;
            var newIndex = args.NewIndex;

            var itemToMove = opRes[oldIndex];
            opRes.RemoveAt(oldIndex);

            if (newIndex < opRes.Count)
            {
                opRes[newIndex].Eval = newIndex;
                opRes.Insert(newIndex, itemToMove);
            }
            else
            {
                opRes[oldIndex].Eval = oldIndex;
                opRes.Add(itemToMove);
            }
            var serializedOtherPageModels = JsonConvert.SerializeObject(opRes);

            await LocalStorageAccessor.SetValueAsync($"{FLangCode}{TLangCode}", serializedOtherPageModels);
        }

        [Parameter] public string FLangCode
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
        [Parameter] public string TLangCode
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

        protected async Task GetOpRes()
        {
            if (Collapsed || opRes != null)
                return;

            var key = $"{FLangCode}{TLangCode}";

            string opStr = await LocalStorageAccessor.GetValueAsync<string>(key);

            List<OtherPageResModel> raw;
            if (!string.IsNullOrWhiteSpace(opStr) && opStr != "null")
            {
                raw = JsonConvert.DeserializeObject<List<OtherPageResModel>>(opStr) ?? new List<OtherPageResModel>();
            }
            else
            {
                raw = await ApiService.GetAsync<List<OtherPageResModel>>($"api/OtherPage/{FLangCode}/{TLangCode}")
                      ?? new List<OtherPageResModel>();

                if (FLangCode != TLangCode)
                {
                    var serialized = JsonConvert.SerializeObject(raw);
                    await LocalStorageAccessor.SetValueAsync(key, serialized);
                }
            }

            opRes = OtherPageService.MakeLinks(raw, Word, FLangCode, TLangCode) ?? new List<OtherPageResModel>();
            langChanged = false;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!Collapsed)
            {
                opRes = null;
                await GetOpRes();
                try
                {
                    FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{FLangCode}{TLangCode}");
                }
                catch (Exception ex)
                {
                    log.LogError($"LocalStorageAccessor.GetValueAsync<string>(fav-{FLangCode}{TLangCode}) " + ex.Message);
                    //throw;
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
