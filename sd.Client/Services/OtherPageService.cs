using Newtonsoft.Json;
using sd.Client.Models;
using sd.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class DictionaryLinksService
{
    private readonly UriService _uriService;
    private readonly LinkParam _reqLinkP;
    private readonly ApiService _apiService;
    private readonly LocalStorageAccessor localStorageAccessor;

    public DictionaryLinksService(UriService uriService, LinkParam reqLinkP, ApiService ApiService, LocalStorageAccessor LocalStorageAccessor)
    {
        _uriService = uriService;
        _reqLinkP = reqLinkP;
        _apiService = ApiService;
        localStorageAccessor = LocalStorageAccessor;
    }
    public async Task<List<DictionaryProviderDto>> GetOpRes(string FLangCode, string TLangCode, string word)
    {
        var key = $"{FLangCode}{TLangCode}";

        string opStr = await localStorageAccessor.GetValueAsync<string>(key);

        List<DictionaryProviderDto> raw;
        if (!string.IsNullOrWhiteSpace(opStr) && opStr != "null")
        {
            raw = JsonConvert.DeserializeObject<List<DictionaryProviderDto>>(opStr) ?? new List<DictionaryProviderDto>();
        }
        else
        {
            raw = await _apiService.GetAsync<List<DictionaryProviderDto>>($"api/OtherPage/{FLangCode}/{TLangCode}")
                  ?? new List<DictionaryProviderDto>();

            if (FLangCode != TLangCode)
            {
                var serialized = JsonConvert.SerializeObject(raw);
                await localStorageAccessor.SetValueAsync(key, serialized);
            }
        }

        return MakeLinks(raw, word, FLangCode, TLangCode) ?? new List<DictionaryProviderDto>();
    }

    public List<DictionaryProviderDto> MakeLinks(List<DictionaryProviderDto> OtherPageModels, 
        string word, string fromLang, string toLang)
    {
        List<DictionaryProviderDto> res = new();
        if (OtherPageModels != null)
        {
            foreach (var otherPage in OtherPageModels)
            {
                if (!string.IsNullOrEmpty(otherPage.Pattern))
                {
                    string newLink = BuildLink(otherPage.Pattern, word, fromLang, toLang);
                    DictionaryProviderDto otherPageResModel = otherPage;
                    otherPageResModel.Link = newLink;
                    res.Add(otherPageResModel);
                }
            }
        }
        return res;
    }

    public string BuildLink(string pattern, string word, string fromLang, string toLang)
    {
        _reqLinkP.Pattern = pattern;
        _reqLinkP.Word = word;
        _reqLinkP.FLangCode = fromLang;
        _reqLinkP.TLangCode = toLang;

        return _uriService.UriBuild(_reqLinkP);
    }
}
