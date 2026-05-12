using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using sd.Shared;
using System.Threading.Tasks;

namespace sd.Client.Features.Settings.Components;

public class DictionaryProviderItemBase : ComponentBase
{
    [Inject] IJSRuntime JsRuntime { set; get; }
    [Parameter] public DictionaryProviderDto dictionaryProvider { get; set; }
    [Parameter] public string FavSite { get; set; }
    [Parameter] public bool CanSetFavSite { get; set; }
    [Parameter] public string FLangCode { get; set; }
    [Parameter] public string TLangCode { get; set; }
    [Parameter] public EventCallback<string> OnFavoriteChanged { get; set; }

    [Parameter] public EventCallback<DictionaryProviderDto> OnLanguageDeleted { get; set; }

    protected bool open = false;

    protected async Task Favorite(string pattern)
    {
        if (CanSetFavSite)
        {
            FavSite = pattern;
            dictionaryProvider.IsFavorite = true;
            if (OnFavoriteChanged.HasDelegate)
                await OnFavoriteChanged.InvokeAsync(pattern);
        }
    }

    protected async Task DeleteLanguage(DictionaryProviderDto op)
    {
        if (OnLanguageDeleted.HasDelegate)
            await OnLanguageDeleted.InvokeAsync(op);
    }

    protected async Task OpenLink(string url)
    {
        await JsRuntime.InvokeVoidAsync("open", url, "_blank"); // window.open
    }
}