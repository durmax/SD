using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using sd.Client.Helpers;
using sd.Client.Models;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class LanguagesBase : ComponentBase
    {
        [Inject] public ILogger<LanguagesBase> Log { get; set; }
        [Inject] LocalStorageAccessor LocalStorageAccessor { get; set; }
        [Inject] IJSRuntime JsRuntime { set; get; }
        [Inject] protected DefaultLangsService DefaultLangsService { get; set; }
        [Inject] KnownLangsService KnownLangsService { get; set; }
        [Inject] public ILanguageContainerService LanguageContainer { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] protected DictionaryLinksService OtherPageService { get; set; }
        [Inject] ILogger<LanguagesBase> log { get; set; }
        [Parameter] public IEnumerable<LangCode> LangCodes { get; set; }

        protected List<DictionaryProviderDto> opRes { get; set; }
        protected List<string> KnownLangs { get; set; }
        private LangCode SFL;
        private LangCode STL;
        private LangCode LToAdd;
        protected string FavSite { get; private set; }

        protected string Fl { get; set; }
        protected string Tl { get; set; }
        //protected string TlT { get; set; }
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

        protected string UILang;

        protected LangCode SelectedTL
        {
            get { return STL; }
            set
            {
                if (value != null)
                {
                    STL = value;
                    LocalStorageAccessor.SetValueAsync("TLang", SelectedTL.Key);
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

            await LocalStorageAccessor.SetValueAsync($"{Fl}{Tl}", serializedOtherPageModels);
        }

        protected void SetUILang()
        {
            try
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(LangCodesHelper.UILangs[UILang]));
                LocalStorageAccessor.SetValueAsync("UILang", LangCodesHelper.UILangs[UILang]);
            }
            catch
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                LocalStorageAccessor.SetValueAsync("UILang", "en-US");

                Log.LogError($"SetUILang: {UILang} not found, set to en-US");
            }
        }

        protected async Task<IEnumerable<LangCode>> SearchLangs(string searchText)
        {
            return await Task.FromResult(LangCodes.Where(x => x.Value.ToLower().Contains(searchText.ToLower())).ToList());
        }

        protected IEnumerable<LangCode> SelectedItemsT { get; set; }  //new List<LangCode>();
        public IEnumerable<LangCode> SelectedItems
        {
            get { return SelectedItemsT; }
            set
            {

                SelectedItemsT = value;
                KnownLangsService.LangsStr = string.Empty;
                foreach (var item in SelectedItemsT)
                {
                    KnownLangsService.LangsStr += "," + item.Key;
                }
                BuildKnownLangs();
            }
        }

        protected async Task OnMotherlanguageChanged(LangCode selectedOption)
        {
            SelectedTL = selectedOption;
            // Handle the selected option change
            await Task.CompletedTask;
        }
        protected async Task OnSecondlanguageChanged(LangCode selectedOption)
        {
            SelectedFL = selectedOption;
            // Handle the selected option change
            await Task.CompletedTask;
        }

        protected async Task OnSearchAsync(OptionsSearchEventArgs<LangCode> e)
        {
            e.Items = LangCodes.Where(i => i.Value.Contains(e.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
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

        protected async Task BuildKnownLangs()
        {
            KnownLangsService.AddKnownLang(SelectedFL?.Key);
            KnownLangsService.AddKnownLang(SelectedTL?.Key);

            KnownLangs = new List<string>();
            KnownLangs = KnownLangsService.KnownLangs;

            await SetLangsStr();
        }

        private async Task SetLangsStr()
        {
            KnownLangsService.LangsStr = null;
            KnownLangs = KnownLangs.Distinct().ToList();

            foreach (var item in KnownLangs)
            {
                KnownLangsService.LangsStr += "," + item;
            }
            await LocalStorageAccessor.SetValueAsync("Langs", KnownLangsService.LangsStr);
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                opRes = await OtherPageService.GetOpRes(Fl, Tl, string.Empty); // ToDo Cash opRes
                FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{Fl}{Tl}");
            }
            catch (Exception ex)
            {
                log.LogError($"LocalStorageAccessor.GetValueAsync<string>(fav-{Fl}{Tl}) " + ex.Message);
                //throw;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await DefaultLangsService.SetDefLangsAsync();

            Fl = DefaultLangsService.DefaultWordLang;
            Tl = DefaultLangsService.DefaultToLang;
            FavSite = await LocalStorageAccessor.GetValueAsync<string>($"fav-{Fl}{Tl}");

            SelectedFL = new LangCode
            {
                Key = Fl,
                Value = Helpers.LangCodesHelper.GetLanguage(Fl)
            };

            SelectedTL = new LangCode
            {
                Key = Tl,
                Value = Helpers.LangCodesHelper.GetLanguage(Tl)
            };

            KnownLangsService.LangsStr = await LocalStorageAccessor.GetValueAsync<string>("Langs");

            await BuildKnownLangs();

            LangCodes = LangCodesHelper.Langs
                .Select(item => new LangCode
                {
                    Key = item.Key,
                    Value = item.Value
                })
                .ToList();

            var lang = await LocalStorageAccessor.GetValueAsync<string>("UILang");
            UILang = LangCodesHelper.UILangs.FirstOrDefault(x => x.Value == lang).Key;

            SelectedItemsT = LangCodes.Where(l => KnownLangs.Contains(l.Key)); //new List<LangCode>();
        }
    }
}