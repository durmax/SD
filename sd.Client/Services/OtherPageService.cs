using Newtonsoft.Json;
using sd.Client.Models;
using sd.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class OtherPageService
{
    private readonly UriService _uriService;
    private readonly LinkParam _reqLinkP;
    private readonly ApiService _apiService;
    private readonly LocalStorageAccessor localStorageAccessor;

    public OtherPageService(UriService uriService, LinkParam reqLinkP, ApiService ApiService, LocalStorageAccessor LocalStorageAccessor)
    {
        _uriService = uriService;
        _reqLinkP = reqLinkP;
        _apiService = ApiService;
        localStorageAccessor = LocalStorageAccessor;
    }

    public List<OtherPageResModel> MakeLinks(List<OtherPageResModel> OtherPageModels, 
        string word, string fromLang, string toLang)
    {
        List<OtherPageResModel> res = new();
        if (OtherPageModels != null)
        {
            foreach (var otherPage in OtherPageModels)
            {
                if (!string.IsNullOrEmpty(otherPage.Pattern))
                {
                    string newLink = BuildLink(otherPage.Pattern, word, fromLang, toLang);
                    OtherPageResModel otherPageResModel = otherPage;
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

    public async Task<List<OtherPageResModel>> GetOpRes(string FLangCode, string TLangCode, string word)
    {
        var key = $"{FLangCode}{TLangCode}";

        string opStr = await localStorageAccessor.GetValueAsync<string>(key);

        List<OtherPageResModel> raw;
        if (!string.IsNullOrWhiteSpace(opStr) && opStr != "null")
        {
            raw = JsonConvert.DeserializeObject<List<OtherPageResModel>>(opStr) ?? new List<OtherPageResModel>();
        }
        else
        {
            raw = await _apiService.GetAsync<List<OtherPageResModel>>($"api/OtherPage/{FLangCode}/{TLangCode}")
                  ?? new List<OtherPageResModel>();

            if (FLangCode != TLangCode)
            {
                var serialized = JsonConvert.SerializeObject(raw);
                await localStorageAccessor.SetValueAsync(key, serialized);
            }
        }

        return MakeLinks(raw, word, FLangCode, TLangCode) ?? new List<OtherPageResModel>();
    }
}
