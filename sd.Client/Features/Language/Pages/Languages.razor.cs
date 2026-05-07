using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using sd.Client.Contracts;
using sd.Client.Features.Language.State;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Features.Language.Pages
{
    public class LanguagesBase : ComponentBase
    {
        [Inject] protected ILogger<LanguagesBase> Log { get; set; } = default!;
        [Inject] protected LocalStorageAccessor LocalStorageAccessor { get; set; } = default!;
        [Inject] protected IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] protected ILanguageContainerService LanguageContainer { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DictionaryLinksService DictionaryLinksService { get; set; } = default!;
        [Inject] protected IKnownLanguagesStore KnownLanguagesStore { get; set; } = default!;

        [Inject] protected UserPreferencesService UserPreferencesService { get; set; } = default!;
        public LanguageSettings LanguageSettings { get; set; }

        // All languages list (dropdown source)
        public IEnumerable<LanguageOption>? LangCodes { get; set; }

        protected IEnumerable<LanguageOption> UiLangItems { get; set; } = LangCodesHelper.UiLangs
                .Select(x => new LanguageOption(x.Key, x.Key));

        // Current dictionary providers (sortable)
        protected List<DictionaryProviderDto> opRes { get; set; } = new();

        // Known languages (for multi-select / tags / etc.)
        protected List<string> KnownLangs { get; set; } = new();

        // UI language key (your UILangs dictionary key, not culture string)
        protected string UILang { get; set; }
        protected LanguageOption SelectedUILang =>
        UiLangItems.FirstOrDefault(x => x.Code == UILang);

        protected IEnumerable<LanguageOption> SelectedItems { get; set; }

        protected LanguagePair ActivePair { get; set; }

        protected string FavSite { get; private set; } = string.Empty;

        protected bool isResetDialogHidden = true;
        protected bool isRemoveAllDialogHidden = true;


        protected async Task SetFavSiteAsync(string fav)
        {
            opRes.ForEach(x => x.IsFavorite = false);
            opRes.FirstOrDefault(x => x.Pattern == fav)?.IsFavorite = true;

            var serialized = JsonConvert.SerializeObject(opRes);
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code), serialized);
        }

        protected async Task OnKnownLangChanged(IEnumerable<LanguageOption> options)
        {
            SelectedItems = options ?? Array.Empty<LanguageOption>();

            LanguageSettings = LanguageSettings with
            {
                KnownLangs = SelectedItems.Select(x => x.Code).Distinct().ToArray()
            };

            await UserPreferencesService.SetSettingsAsync(LanguageSettings);
        }

        protected async Task Reverse()
        {
            ActivePair = new LanguagePair(ActivePair.To, ActivePair.From);
            await ReloadProvidersAsync();
        }


        protected override async Task OnInitializedAsync()
        {
            //await DefaultLangsService.SetDefLangsAsync();

            var fromCode = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FromLang) ?? "en";
            var toCode = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.ToLang) ?? "de";

            ActivePair = new LanguagePair(
                new LanguageOption(fromCode, LangCodesHelper.GetLanguageNameOrEmpty(fromCode)),
                new LanguageOption(toCode, LangCodesHelper.GetLanguageNameOrEmpty(toCode)));

            LangCodes ??= LangCodesHelper.Langs
                .Select(x => new LanguageOption(x.Key, x.Value));

            LanguageSettings = await UserPreferencesService.GetSettingsAsync(true);

            // UI language initial selection
            var culture = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.UiLang);
            if (string.IsNullOrEmpty(culture))
            {
                culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
                await LocalStorageAccessor.SetValueAsync(LangStorageKeys.UiLang, culture);
            }
            UILang = LangCodesHelper.UiLangs.FirstOrDefault(x => x.Value == culture).Key;

            SelectedItems = (LangCodes ?? Array.Empty<LanguageOption>())
                                .Where(l => LanguageSettings.KnownLangs.Contains(l.Code))
                                .ToList();

            // Load dictionary providers + favorite site
            await ReloadProvidersAsync();
        }

        protected async Task ReloadProvidersAsync()
        {
            try
            {
                opRes = await DictionaryLinksService.GetDictionaryProviders(
                    ActivePair.From.Code,
                    ActivePair.To.Code,
                    string.Empty) ?? new List<DictionaryProviderDto>();

                FavSite = opRes.FirstOrDefault(x => x.IsFavorite)?.Pattern ?? FavSite;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"ReloadProvidersAsync failed for {ActivePair.From.Code}-{ActivePair.To.Code}");
            }
        }

        protected async Task OnActivePairChanged(LanguagePair pair)
        {
            ActivePair = pair;
            //DefaultLangsService.DefaultWordLang = pair.From.Code;

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.ToLang, pair.To.Code);
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.FromLang, pair.From.Code);

            await ReloadProvidersAsync();
        }

        protected async Task OnUILangChanged(LanguageOption option)
        {
            UILang = option.Code;
            var fallbackCulture = "en-US";

            try
            {
                var culture = (UILang != null && LangCodesHelper.UiLangs.TryGetValue(UILang, out var c))
                    ? c
                    : fallbackCulture;

                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(culture));
                await LocalStorageAccessor.SetValueAsync(LangStorageKeys.UiLang, culture);
            }
            catch (Exception ex)
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(fallbackCulture));
                await LocalStorageAccessor.SetValueAsync(LangStorageKeys.UiLang, fallbackCulture);

                Log.LogError(ex, $"SetUILangAsync: '{UILang}' not found, set to {fallbackCulture}");
            }
        }

        protected async Task SortListAsync(FluentSortableListEventArgs args)
        {
            if (args is null || args.OldIndex == args.NewIndex) return;
            if (opRes is null || opRes.Count == 0) return;
            if (args.OldIndex < 0 || args.OldIndex >= opRes.Count) return;
            if (args.NewIndex < 0) return;

            var item = opRes[args.OldIndex];
            opRes.RemoveAt(args.OldIndex);

            var insertIndex = Math.Min(args.NewIndex, opRes.Count);
            opRes.Insert(insertIndex, item);

            for (int i = 0; i < opRes.Count; i++)
                opRes[i].Eval = i;

            var serialized = JsonConvert.SerializeObject(opRes);
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code), serialized);
        }

        protected async Task OnSearchAsync(OptionsSearchEventArgs<LanguageOption> e)
        {
            if (LangCodes is null)
            {
                e.Items = Array.Empty<LanguageOption>();
                return;
            }

            e.Items = LangCodes
                .Where(i => i.Name.Contains(e.Text ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            await Task.CompletedTask;
        }

        protected async Task ResetOPAsync()
        {
            isResetDialogHidden = false;

            await LocalStorageAccessor.RemoveAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code));
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }

        protected async Task RemoveAllDataAsync()
        {
            isRemoveAllDialogHidden = false;

            await LocalStorageAccessor.Clear();
            await JsRuntime.InvokeVoidAsync("alert", "Your data are deleted");

            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}