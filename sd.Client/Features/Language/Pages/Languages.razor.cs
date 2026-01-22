using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using sd.Client.Features.Language.State;
using sd.Client.Helpers;
using sd.Client.Features.Language.Contracts;
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
        [Inject] protected DefaultLangsService DefaultLangsService { get; set; } = default!;
        [Inject] protected ILanguageContainerService LanguageContainer { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DictionaryLinksService DictionaryLinksService { get; set; } = default!;
        [Inject] protected IKnownLanguagesStore KnownLanguagesStore { get; set; } = default!;

        // All languages list (dropdown source)
        public IEnumerable<LanguageOption>? LangCodes { get; set; }

        protected IEnumerable<LanguageOption> UiLangItems { get; set; }

        // Current dictionary providers (sortable)
        protected List<DictionaryProviderDto> opRes { get; set; } = new();

        // Known languages (for multi-select / tags / etc.)
        protected List<string> KnownLangs { get; set; } = new();

        // UI language key (your UILangs dictionary key, not culture string)
        protected string? UILang { get; set; }

        private bool _selectedItemsInitialized;

        protected IEnumerable<LanguageOption> SelectedItems { get; set; }

        private bool _suppressSelectedItemsChanged = true;
        private bool _isPersistingSelectedItems;

        protected LanguageOption FromLanguage { get; set; }
        protected LanguageOption ToLanguage { get; set; }

        protected string FavSite { get; private set; } = string.Empty;

        protected async Task OnSelectedOptionsChanged(IEnumerable<LanguageOption> options)
        {
            if (_suppressSelectedItemsChanged) return;
            if (_isPersistingSelectedItems) return;

            try
            {
                _isPersistingSelectedItems = true;

                SelectedItems = options ?? Array.Empty<LanguageOption>();

                KnownLangs = SelectedItems.Select(x => x.Code).Distinct().ToList();
                KnownLanguagesStore.EnsureContains(KnownLangs, FromLanguage.Code, ToLanguage.Code);

                await KnownLanguagesStore.SaveAsync(KnownLangs);
            }
            finally
            {
                _isPersistingSelectedItems = false;
            }
        }

        protected async Task Reverse()
        {
            (FromLanguage, ToLanguage) = (ToLanguage, FromLanguage);
            await ReloadProvidersAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            await DefaultLangsService.SetDefLangsAsync();

            var fromCode = DefaultLangsService.DefaultWordLang ?? "en";
            var toCode = DefaultLangsService.DefaultToLang ?? "de";

            FromLanguage = new LanguageOption(
                fromCode,
                LangCodesHelper.GetLanguage(fromCode)
            );

            ToLanguage = new LanguageOption(
                toCode,
                LangCodesHelper.GetLanguage(toCode)
            );

            LangCodes ??= LangCodesHelper.Langs
                .Select(x => new LanguageOption(x.Key, x.Value));

            // Load known langs from storage (via store)
            KnownLangs = await KnownLanguagesStore.GetAsync();

            // Ensure current FL/TL are included + persist back
            KnownLanguagesStore.EnsureContains(KnownLangs, FromLanguage.Code, ToLanguage.Code);
            await KnownLanguagesStore.SaveAsync(KnownLangs);


            UiLangItems = LangCodesHelper.UILangs
                .Select(x => new LanguageOption(x.Key, x.Key));

            // UI language initial selection
            var culture = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.UiLang);
            UILang = LangCodesHelper.UILangs.FirstOrDefault(x => x.Value == culture).Key;

            if (!_selectedItemsInitialized)
            {
                _suppressSelectedItemsChanged = true;

                SelectedItems = (LangCodes ?? Array.Empty<LanguageOption>())
                    .Where(l => KnownLangs.Contains(l.Code))
                    .ToList();

                _suppressSelectedItemsChanged = false;
                _selectedItemsInitialized = true;
            }

            // Load dictionary providers + favorite site
            await ReloadProvidersAsync();
        }

        protected async Task ReloadProvidersAsync()
        {
            try
            {
                opRes = await DictionaryLinksService.GetOpRes(FromLanguage.Code, ToLanguage.Code, string.Empty) ?? new List<DictionaryProviderDto>(); // :contentReference[oaicite:2]{index=2}
                FavSite = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FavoriteSite(FromLanguage.Code, ToLanguage.Code)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"ReloadProvidersAsync failed for {FromLanguage.Code}-{ToLanguage.Code}");
            }
        }

        protected async Task OnMotherlanguageChanged(string? langName)
        {
            if (string.IsNullOrWhiteSpace(langName)) return;

            var code = LangCodesHelper.GetLanguageCode(langName);
            ToLanguage = new LanguageOption(code, langName);

            await PersistLanguageChange(
                code,
                LangStorageKeys.ToLang,
                setAsDefaultWordLang: false
            );
        }

        protected async Task OnSecondlanguageChanged(string? langName)
        {
            if (string.IsNullOrWhiteSpace(langName)) return;

            var code = LangCodesHelper.GetLanguageCode(langName);
            FromLanguage = new LanguageOption(code, langName);

            await PersistLanguageChange(
                code,
                LangStorageKeys.FromLang,
                setAsDefaultWordLang: true
            );
        }

        private async Task PersistLanguageChange(
            string langCode,
            string storageKey,
            bool setAsDefaultWordLang)
        {
            if (setAsDefaultWordLang)
                DefaultLangsService.DefaultWordLang = langCode;

            KnownLanguagesStore.EnsureContains(KnownLangs, langCode);

            await LocalStorageAccessor.SetValueAsync(storageKey, langCode);
            await KnownLanguagesStore.SaveAsync(KnownLangs);

            await ReloadProvidersAsync();
        }

        protected async Task OnUILangChanged(string? value)
        {
            UILang = value;
            var fallbackCulture = "en-US";

            try
            {
                var culture = (UILang != null && LangCodesHelper.UILangs.TryGetValue(UILang, out var c))
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
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(FromLanguage.Code, ToLanguage.Code), serialized);
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
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"You try to reset {FromLanguage.Code}-{ToLanguage.Code}, are you sure?");
            if (!confirmed) return;

            await LocalStorageAccessor.RemoveAsync(LangStorageKeys.DictionaryOrder(FromLanguage.Code, ToLanguage.Code));
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }

        protected async Task RemoveAllDataAsync()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "You try to delete all data, are you sure?");
            if (!confirmed) return;

            await LocalStorageAccessor.Clear();
            await JsRuntime.InvokeVoidAsync("alert", "Your data are deleted");

            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }
}