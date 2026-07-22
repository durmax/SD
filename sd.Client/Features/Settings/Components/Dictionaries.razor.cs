using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using sd.Client.Contracts;
using sd.Shared;
using System.Collections.Generic;

namespace sd.Client.Features.Settings.Components
{
    public class DictionariesBase : ComponentBase
    {
        [Parameter] public List<DictionaryProviderDto> DictionaryProviders { get; set; } = new();
        [Parameter] public IEnumerable<LanguageOption> SelectedItems { get; set; } = new List<LanguageOption>();
        [Parameter] public LanguagePair ActivePair { get; set; } = default!;
        [Parameter] public string FavSite { get; set; } = string.Empty;

        [Parameter] public EventCallback<string> SetFavSiteAsync { get; set; }
        [Parameter] public EventCallback<LanguagePair> PairChanged { get; set; }
        [Parameter] public EventCallback Reverse { get; set; }
        [Parameter] public EventCallback<FluentSortableListEventArgs> SortListAsync { get; set; }
        [Parameter] public EventCallback<DictionaryProviderDto> RemoveItemAsync { get; set; }
    }
}