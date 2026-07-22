using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using sd.Client.Contracts;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Features.Vocab.Components
{
    public class DictionaryLinksMenuBase : ComponentBase
    {
        [Inject] DictionaryLinksService DictionaryLinksService { set; get; }
        [Inject] IJSRuntime JsRuntime { set; get; }
        [Inject] ILogger<WordBase> Log { get; set; }
        [Inject] UserPreferencesService UserPreferencesService { get; set; }

        [Parameter] public WordDto WordDto { get; set; }

        // New parameters to support pair selector
        public IEnumerable<LanguageOption> LanguageOptions { get; set; } = new List<LanguageOption>();
        public LanguagePair Pair { get; set; }
        [Parameter] public EventCallback<LanguagePair> PairChanged { get; set; }
        [Parameter] public EventCallback Reverse { get; set; }

        public LanguageSettings? LanguageSettings { get; set; }

        protected bool open = false;
        protected List<DictionaryProviderDto> dictProviders { get; set; } = new();

        protected async Task OpenLink(string url)
        {
            open = false;                    // close menu deterministically
            await JsRuntime.InvokeVoidAsync("open", url, "_blank"); // window.open
        }

        protected override async Task OnInitializedAsync()
        {
            LanguageSettings = await UserPreferencesService.GetSettingsAsync(false);

            LanguageOptions = LanguageSettings.KnownLangs.Select(code => new LanguageOption { Code = code, Name = LangCodesHelper.GetLanguageNameOrEmpty(code) }).ToList();

            Pair = new LanguagePair
            {
                From = new LanguageOption
                {
                    Code = WordDto?.WordLang,
                    Name = LangCodesHelper.GetLanguageNameOrEmpty(WordDto?.WordLang)
                },
                To = new LanguageOption
                {
                    Code = WordDto.ToLang,
                    Name = LangCodesHelper.GetLanguageNameOrEmpty(WordDto?.ToLang)
                }
            };
        }

        protected override async Task OnParametersSetAsync()
        {
            var from = Pair.From.Code ?? WordDto?.WordLang;
            var to = Pair.To.Code ?? WordDto?.ToLang;

            dictProviders = await DictionaryLinksService.GetDictionaryProviders(from, to, WordDto?.Title);
        }

        protected async Task OnPairChangedAsync(LanguagePair newPair)
        {
            Pair = newPair;
            if (PairChanged.HasDelegate)
                await PairChanged.InvokeAsync(newPair);

            // reload providers for the new pair
            dictProviders = await DictionaryLinksService.GetDictionaryProviders(Pair.From.Code, Pair.To.Code, WordDto?.Title);
            StateHasChanged();
        }

        protected async Task OnReverseAsync()
        {
            if (Reverse.HasDelegate)
                await Reverse.InvokeAsync();
            else if (Pair != null)
                Pair = new LanguagePair(Pair.To, Pair.From);

            // reload providers after reverse
            var from = Pair.From.Code ?? WordDto?.WordLang;
            var to = Pair.To.Code ?? WordDto?.ToLang;
            dictProviders = await DictionaryLinksService.GetDictionaryProviders(from, to, WordDto?.Title);
            StateHasChanged();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                // Log the initial state for debugging
                Log.LogInformation("DictionaryLinksMenu initialized with WordId: {WordId}, Initial Pair: {From} -> {To}",
                    WordDto?.WordId, Pair.From.Code, Pair.To.Code);
            }
        }
    }
}
