using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using sd.Client.Features.Language.Contracts;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System.Threading.Tasks;

namespace sd.Client.Features.Language.Components;

public class OtherPageBase : ComponentBase
{
    [Inject] LocalStorageAccessor LocalStorageAccessor { get; set; }
    [Parameter] public DictionaryProviderDto otherPage { get; set; }        
    [Parameter] public string FavSite { get; set; }
    [Parameter] public bool CanSetFavSite { get; set; }
    [Parameter] public string FLangCode { get; set; }
    [Parameter] public string TLangCode { get; set; }

    public string Info { get; private set; }

    protected async Task Favorite(DictionaryProviderDto otherPage)
    {
        if (CanSetFavSite || string.IsNullOrWhiteSpace(FavSite))
        {
            await LocalStorageAccessor.SetValueAsync(LangStorageKeys.FavoriteSite(FLangCode,TLangCode), otherPage.Pattern);
            FavSite = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FavoriteSite(FLangCode, TLangCode));
        }
        else
        {
            Info = "/Languages";
        }
    }
}