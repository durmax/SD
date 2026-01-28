using Microsoft.AspNetCore.Components;
using sd.Client.Services;
using sd.Shared;
using System.Threading.Tasks;

namespace sd.Client.Features.Language.Components;

public class OtherPageBase : ComponentBase
{
    [Inject] DefaultLangsService DefaultLangsService { get; set; }
    [Parameter] public DictionaryProviderDto otherPage { get; set; }        
    [Parameter] public string FavSite { get; set; }
    [Parameter] public bool CanSetFavSite { get; set; }
    [Parameter] public string FLangCode { get; set; }
    [Parameter] public string TLangCode { get; set; }

    public string Info { get; private set; }

    protected async Task Favorite(string pattern)
    {
        if (CanSetFavSite || string.IsNullOrWhiteSpace(FavSite))
        {
            await DefaultLangsService.SetFavorite(FLangCode,TLangCode, pattern);
            FavSite = pattern;
        }
        else
        {
            Info = "/Languages";
        }
    }
}