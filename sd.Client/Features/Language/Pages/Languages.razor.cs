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

namespace sd.Client.Features.Language.Pages
{
    public class LanguagesBase : ComponentBase
    {
        [Inject] protected ILogger<LanguagesBase> Log { get; set; } = default!;
        [Inject] protected LocalStorageAccessor LocalStorageAccessor { get; set; } = default!;
        [Inject] protected IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] protected DefaultLangsService DefaultLangsService { get; set; } = default!;
        [Inject] protected KnownLangsService KnownLangsService { get; set; } = default!;
        [Inject] protected ILanguageContainerService LanguageContainer { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DictionaryLinksService OtherPageService { get; set; } = default!;

        // All languages list (dropdown source)
        [Parameter] public IEnumerable<LangCode>? LangCodes { get; set; }

        protected List<LangCode> UiLangItems { get; set; } = new();

        // Current dictionary providers (sortable)
        protected List<DictionaryProviderDto> opRes { get; set; } = new();

        // Known languages (for multi-select / tags / etc.)
        protected List<string> KnownLangs { get; set; } = new();

        // UI language key (your UILangs dictionary key, not culture string)
        protected string? UILang { get; set; }

        // Mother / Second language selection
        protected LangCode? SelectedFL { get; set; } // Second language (learn)
        protected LangCode? SelectedTL { get; set; } // Mother language

        // Selected items for known langs picker
        private List<LangCode> _selectedItems = new();
        protected List<LangCode> SelectedItemsT
        {
            get => _selectedItems;
            set => _selectedItems = value ?? new List<LangCode>();
        }


        private bool _selectedItemsInitialized;

        protected List<LangCode> SelectedItems { get; set; } = new();

        private bool _suppressSelectedItemsChanged = true;
        private bool _isPersistingSelectedItems;

        protected async Task OnSelectedOptionsChanged(IEnumerable<LangCode> options)
        {
            if (_suppressSelectedItemsChanged) return;
            if (_isPersistingSelectedItems) return;

            try
            {
                _isPersistingSelectedItems = true;

                SelectedItems = options?.ToList() ?? new List<LangCode>();

                KnownLangs = SelectedItems.Select(x => x.Key).Distinct().ToList();
                EnsureKnownLang(Fl);
                EnsureKnownLang(Tl);

                await PersistKnownLangsAsync();
            }
            finally
            {
                _isPersistingSelectedItems = false;
            }
        }


        // Active lang codes used for dictionary links ordering keys etc.
        protected string Fl { get; set; } = "en";
        protected string Tl { get; set; } = "de";

        protected string FavSite { get; private set; } = string.Empty;

        protected void Reverse() => (Tl, Fl) = (Fl, Tl);

        // --------------------------
        // Initialization / lifecycle
        // --------------------------

        protected override async Task OnInitializedAsync()
        {
            await DefaultLangsService.SetDefLangsAsync(); // :contentReference[oaicite:1]{index=1}

            Fl = DefaultLangsService.DefaultWordLang ?? "en";
            Tl = DefaultLangsService.DefaultToLang ?? "de";

            // Build LangCodes list if not provided as parameter
            LangCodes ??= LangCodesHelper.Langs
                .Select(x => new LangCode { Key = x.Key, Value = x.Value })
                .ToList();

            // Set initial selections
            SelectedFL = new LangCode { Key = Fl, Value = LangCodesHelper.GetLanguage(Fl) };
            SelectedTL = new LangCode { Key = Tl, Value = LangCodesHelper.GetLanguage(Tl) };

            // Load known langs from storage
            KnownLangsService.LangsStr = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.KnownLangs);
            KnownLangs = ParseLangsStr(KnownLangsService.LangsStr);

            // Ensure current FL/TL are included
            EnsureKnownLang(Fl);
            EnsureKnownLang(Tl);
            await PersistKnownLangsAsync();

            UiLangItems = LangCodesHelper.UILangs
                .Select(x => new LangCode { Key = x.Key, Value = x.Key })
                .ToList();

            // UI language initial selection
            var culture = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.UiLang);
            UILang = LangCodesHelper.UILangs.FirstOrDefault(x => x.Value == culture).Key;

            if (!_selectedItemsInitialized)
            {
                // Preselect known langs in UI
                var known = new HashSet<string>(KnownLangs);
            _suppressSelectedItemsChanged = true;

                SelectedItems.Clear();
                SelectedItems.AddRange(
                    (LangCodes ?? Array.Empty<LangCode>())
                        .Where(l => KnownLangs.Contains(l.Key))
                );
                //                SelectedItems.AddRange(new[]
                //{
                //    new LangCode { Key="en", Value="English" },
                //    new LangCode { Key="de", Value="German" }
                //});


                _suppressSelectedItemsChanged = false;
                _selectedItemsInitialized = true;
            }

            // Load dictionary providers + favorite site
            await ReloadProvidersAsync();
        }

        protected async Task OnUILangChanged(string? value)
        {
            UILang = value;
            await SetUILangAsync();
        }

        private async Task ReloadProvidersAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Fl) && Fl.Length > 2) Fl = LangCodesHelper.GetLanguageCode(Fl);
                if (!string.IsNullOrWhiteSpace(Tl) && Tl.Length > 2) Tl = LangCodesHelper.GetLanguageCode(Tl);

                opRes = await OtherPageService.GetOpRes(Fl, Tl, string.Empty) ?? new List<DictionaryProviderDto>(); // :contentReference[oaicite:2]{index=2}
                FavSite = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FavoriteSite(Fl, Tl)) ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"ReloadProvidersAsync failed for {Fl}-{Tl}");
            }
        }

        // --------------------------
        // Mother / Second language
        // --------------------------

        // Mother language (TL)
        protected async Task OnMotherlanguageChanged(LangCode selectedOption)
        {
            if (selectedOption is null) return;

            SelectedTL = selectedOption;
            Tl = selectedOption.Key;

            DefaultLangsService.DefaultToLang = Tl;
            EnsureKnownLang(Tl);

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.ToLang, Tl);
            await PersistKnownLangsAsync();
            await ReloadProvidersAsync();
        }

        // Second language (FL)
        protected async Task OnSecondlanguageChanged(LangCode selectedOption)
        {
            if (selectedOption is null) return;

            SelectedFL = selectedOption;
            Fl = selectedOption.Key;

            DefaultLangsService.DefaultWordLang = Fl;
            EnsureKnownLang(Fl);

            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.FromLang, Fl);
            await PersistKnownLangsAsync();
            await ReloadProvidersAsync();
        }

        protected async Task OnMotherlanguageKeyChanged(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            Tl = key;

            key = LangCodesHelper.GetLanguageCode(key);
            // keep Tl in sync for UI immediately
            // find the LangCode instance from the Items list (safe even if null)
            var opt = SelectedItems.FirstOrDefault(x => x.Key == key)
                      ?? new LangCode { Key = key, Value = LangCodesHelper.GetLanguage(key) };

            await OnMotherlanguageChanged(opt);
        }

        protected async Task OnSecondlanguageKeyChanged(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            Fl = key;

            key = LangCodesHelper.GetLanguageCode(key);

            var opt = SelectedItems.FirstOrDefault(x => x.Key == key)
                      ?? new LangCode { Key = key, Value = LangCodesHelper.GetLanguage(key) };

            await OnSecondlanguageChanged(opt);
        }

        // --------------------------
        // UI language
        // --------------------------

        protected async Task SetUILangAsync()
        {
            // UILang is the key in LangCodesHelper.UILangs (like "English", "Deutsch", etc.)
            // Value is the culture string like "en-US"
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

        // --------------------------
        // Known languages
        // --------------------------

        private void EnsureKnownLang(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            if (!KnownLangs.Contains(code))
                KnownLangs.Add(code);
        }

        private async Task PersistKnownLangsAsync()
        {
            KnownLangs = KnownLangs.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            // Store as ",en,de,ar" (keeps your existing format)
            KnownLangsService.LangsStr = string.Join("", KnownLangs.Select(x => "," + x));
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.KnownLangs, KnownLangsService.LangsStr);
        }

        private static List<string> ParseLangsStr(string? langsStr)
        {
            if (string.IsNullOrWhiteSpace(langsStr) || langsStr == "null")
                return new List<string>();

            // Existing format begins with commas
            return langsStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();
        }

        // --------------------------
        // DictionaryLinks sorting
        // --------------------------

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

            // Optional: normalize Eval to match order
            for (int i = 0; i < opRes.Count; i++)
                opRes[i].Eval = i;

            var serialized = JsonConvert.SerializeObject(opRes);
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.DictionaryOrder(Fl, Tl), serialized);
        }

        // --------------------------
        // UI helpers
        // --------------------------

        protected async Task OnSearchAsync(OptionsSearchEventArgs<LangCode> e)
        {
            if (LangCodes is null)
            {
                e.Items = Array.Empty<LangCode>();
                return;
            }

            e.Items = LangCodes
                .Where(i => i.Value.Contains(e.Text ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            await Task.CompletedTask;
        }

        protected async Task ResetOPAsync()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"You try to reset {Fl}-{Tl}, are you sure?");
            if (!confirmed) return;

            await LocalStorageAccessor.RemoveAsync(LangStorageKeys.DictionaryOrder(Fl, Tl));
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