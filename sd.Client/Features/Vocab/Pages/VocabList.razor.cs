using AKSoftware.Localization.MultiLanguages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using sd.Client.Features.Vocab.State;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System.Threading.Tasks;

namespace sd.Client.Features.Vocab.Pages;

public class VocabListBase : ComponentBase
{

    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public LocalStorageAccessor LocalStorageAccessor { get; set; } = default!;
    [Inject] public ILanguageContainerService LanguageContainer { get; set; } = default!;
    [Inject] public ILogger<VocabListBase> Log { get; set; } = default!;
    [Inject] public VocabStore Store { get; set; } = default!;
    [Inject] protected UserPreferencesService UserPreferencesService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Store.EnsureDraftRow();

        var uiLang = UserPreferencesService.GetSettingsAsync(false).Result.UiLang;

        if (!string.IsNullOrWhiteSpace(uiLang) && uiLang != "null")
        {
            try
            {
                LanguageContainer.SetLanguage(System.Globalization.CultureInfo.GetCultureInfo(uiLang));
            }
            catch
            {
                Log.LogError($"SetLanguage for uiLang: {uiLang}");
            }
        }
    }

    protected async Task LoadMore()
    {
        await Store.LoadNextPageAsync(pageSize: 10);
        StateHasChanged();
    }

    protected async Task OnWordSave(WordDto word)
    {
        await Store.UpsertFromSave(word);
        StateHasChanged();
    }

    protected void OnWordFound(WordDto oldWord)
    {
        Store.AddIfMissing(oldWord);
        StateHasChanged();
    }

    protected void OnWordDelete(WordDto word)
    {
        Store.Remove(word);
        StateHasChanged();
    }
}
