using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using sd.Client.Contracts;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Features.Settings.Pages
{
    public class SettingsBase : ComponentBase
    {
        [Inject] protected ILogger<SettingsBase> Log { get; set; } = default!;
        [Inject] protected LocalStorageAccessor LocalStorageAccessor { get; set; } = default!;
        [Inject] protected IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] protected ILanguageContainerService LanguageContainer { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DictionaryLinksService DictionaryLinksService { get; set; } = default!;
        [Inject] protected UserPreferencesService UserPreferencesService { get; set; } = default!;
        public LanguageSettings? LanguageSettings { get; set; } = LanguageSettings.Default;

        // All languages list (dropdown source)
        public IEnumerable<LanguageOption>? LangCodes { get; set; }

        protected IEnumerable<LanguageOption> UiLangItems { get; set; } = LangCodesHelper.UiLangs
            .Select(x => new LanguageOption(x.Key, x.Value))
            .ToList();

        // Current dictionary providers (sortable)
        protected List<DictionaryProviderDto> dictionaryProviders { get; set; } = new();

        // UI language key (your UILangs dictionary key, not culture string)
        // protected string UILang { get; set; }
        protected LanguageOption SelectedUILang { get; set; }

        protected IEnumerable<LanguageOption> SelectedItems { get; set; }

        protected LanguagePair ActivePair { get; set; }

        protected string FavSite { get; private set; } = string.Empty;

        protected bool isResetDialogHidden = true;
        protected bool isRemoveAllDialogHidden = true;

        protected async Task SetFavSiteAsync(string fav)
        {
            dictionaryProviders.ForEach(x => x.IsFavorite = false);
            dictionaryProviders.FirstOrDefault(x => x.Pattern == fav)?.IsFavorite = true;

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code), dictionaryProviders);
        }

        protected async Task OnKnownLangChanged(IEnumerable<LanguageOption> options)
        {
            SelectedItems = options ?? Array.Empty<LanguageOption>();

            LanguageSettings = LanguageSettings with
            {
                KnownLangs = SelectedItems.Select(x => x.Code).Distinct().ToArray()
            };

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.LanguageSettings, LanguageSettings);
        }

        protected async Task Reverse()
        {
            ActivePair = new LanguagePair(ActivePair.To, ActivePair.From);
            await ReloadProvidersAsync();
        }


        protected override async Task OnInitializedAsync()
        {
            LanguageSettings = await UserPreferencesService.GetSettingsAsync(false)
                   ?? LanguageSettings.Default;

            LangCodes = LangCodesHelper.Langs
                .Select(x => new LanguageOption(x.Key, x.Value))
                .ToList();

            ActivePair = new LanguagePair(
                new LanguageOption(LanguageSettings.FromLang, LangCodesHelper.GetLanguageNameOrEmpty(LanguageSettings.FromLang)),
                new LanguageOption(LanguageSettings.ToLang, LangCodesHelper.GetLanguageNameOrEmpty(LanguageSettings.ToLang)));

            SelectedUILang = UiLangItems.FirstOrDefault(x => x.Code == LanguageSettings.UiLangCode);
            
            SelectedItems = LangCodes
    .Where(l => LanguageSettings.KnownLangs.Contains(l.Code))
    .ToList();

            // UI language initial selection
            if (LangCodesHelper.GetUiCulture(LanguageSettings.UiLangCode, out var culture))
            {
                LanguageContainer.SetLanguage(culture!);
            }

            // Load dictionary providers + favorite site
            await ReloadProvidersAsync();
        }

        protected async Task ReloadProvidersAsync()
        {
            try
            {
                dictionaryProviders = await DictionaryLinksService.GetDictionaryProviders(
                    ActivePair.From.Code,
                    ActivePair.To.Code,
                    string.Empty) ?? new List<DictionaryProviderDto>();

                FavSite = dictionaryProviders.FirstOrDefault(x => x.IsFavorite)?.Pattern ?? FavSite;
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

            LanguageSettings = LanguageSettings with
            {
                ToLang = pair.To.Code,
                FromLang = pair.From.Code
            };
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.LanguageSettings, LanguageSettings);

            await ReloadProvidersAsync();
        }

        protected async Task OnUILangChanged(LanguageOption option)
        {
            if (option.Code == SelectedUILang.Code)
                return;

            SelectedUILang = option;

            LanguageSettings = LanguageSettings! with
            {
                UiLangCode = option.Code
            };

            await UserPreferencesService.SetSettingsAsync(LanguageSettings);

            var fallbackUiLangCode = "en";

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.LanguageSettings, LanguageSettings);

            try
            {
                if (LangCodesHelper.GetUiCulture(option.Code, out var culture))
                {
                    LanguageContainer.SetLanguage(culture);
                }
            }
            catch (Exception ex)
            {
                if (LangCodesHelper.GetUiCulture(fallbackUiLangCode, out var culture))
                {
                    LanguageContainer.SetLanguage(culture);
                }

                Log.LogError(ex, "SetUILangAsync failed for '{UiLangCode}', fallback to {FallbackUiLangCode}",
                    option.Code, fallbackUiLangCode);
            }
        }

        protected async Task SortListAsync(FluentSortableListEventArgs args)
        {
            if (args is null || args.OldIndex == args.NewIndex) return;
            if (dictionaryProviders is null || dictionaryProviders.Count == 0) return;
            if (args.OldIndex < 0 || args.OldIndex >= dictionaryProviders.Count) return;
            if (args.NewIndex < 0) return;

            var item = dictionaryProviders[args.OldIndex];
            dictionaryProviders.RemoveAt(args.OldIndex);

            var insertIndex = Math.Min(args.NewIndex, dictionaryProviders.Count);
            dictionaryProviders.Insert(insertIndex, item);

            for (int i = 0; i < dictionaryProviders.Count; i++)
                dictionaryProviders[i].Eval = i;

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code), dictionaryProviders);
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

        protected async Task RemoveItemAsync(DictionaryProviderDto item)
        {
            dictionaryProviders.Remove(item);
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(ActivePair.From.Code, ActivePair.To.Code), dictionaryProviders);
            StateHasChanged();
        }
    }
}