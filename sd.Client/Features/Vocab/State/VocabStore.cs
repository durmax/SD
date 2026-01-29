using sd.Client.Features.Vocab.Api;
using sd.Client.Helpers;
using sd.Client.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Client.Features.Vocab.State;

public sealed class VocabStore
{
    private readonly VocabApiClient _api;
    private readonly LocalStorageAccessor LocalStorageAccessor;

    public VocabStore(VocabApiClient api, LocalStorageAccessor localStorageAccessor)
    {
        _api = api;
        LocalStorageAccessor = localStorageAccessor;
    }

    public List<WordDto> Items { get; } = new();

    public bool IsLoading { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int NewWords { get; private set; }

    public async Task EnsureDraftRow()
    {
        if (Items.Count > 0) return;

        Items.Insert(0, await NewDraftAsync());
    }

    public async Task AddNewDraftRow()
    {
        NewWords++;
        Items.Insert(0, await NewDraftAsync(NewWords.ToString()));
    }

    public async Task LoadNextPageAsync(int pageSize = 10)
    {
        IsLoading = true;

        var before = Items.Count;
        var page = await _api.GetPageWordsAsync(skip: 0, take: pageSize, page: CurrentPage);
        Items.AddRange(page);

        // Keep your existing behavior (even if it’s a bit unusual):
        // currentPage += (newCount - oldCount)
        var delta = Items.Count - before;
        CurrentPage += delta;

        IsLoading = false;
    }

    public async Task UpsertFromSave(WordDto word)
    {
        if (word == null || string.IsNullOrWhiteSpace(word.WordId))
            return;

        var existing = Items.FirstOrDefault(w => w.WordId == word.WordId);
        if (existing != null)
        {
            var index = Items.IndexOf(existing);
            Items[index] = word;
        }
        else
        {
            Items.Add(word);
        }

        await AddNewDraftRow();
    }

    public void AddIfMissing(WordDto word)
    {
        if (word == null || string.IsNullOrWhiteSpace(word.WordId))
            return;

        if (!Items.Exists(w => w.WordId == word.WordId))
            Items.Add(word);
    }

    public void Remove(WordDto word)
    {
        var idx = Items.FindIndex(w => Equals(w, word));
        if (idx >= 0)
        {
            Items.RemoveAt(idx);
            CurrentPage--;
        }
    }

    private async Task<WordDto> NewDraftAsync(string? id = null)
        => new()
        {
            WordId = id ?? NewWords.ToString(),
            WordLang = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.FromLang),
            ToLang = await LocalStorageAccessor.GetValueAsync<string>(LangStorageKeys.ToLang),
            ShareWith = ShareWith.Public
        };
}
