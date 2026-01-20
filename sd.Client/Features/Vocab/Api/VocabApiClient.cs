using sd.Client.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Features.Vocab.Api;

public sealed class VocabApiClient
{
    private readonly ApiService _api;

    public VocabApiClient(ApiService api) => _api = api;

    public Task<List<WordDto>> GetPageWordsAsync(int skip, int take, int page)
        => _api.GetAsync<List<WordDto>>($"api/Word/GetPageWords/{skip}/{take}/{page}");
}
