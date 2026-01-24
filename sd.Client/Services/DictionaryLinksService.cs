using Newtonsoft.Json;
using sd.Client.Helpers;
using sd.Client.Models;
using sd.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Client.Services;

public class DictionaryLinksService
{
    private readonly LinkModel _linkModel;
    private readonly ApiService _apiService;
    private readonly LocalStorageAccessor localStorageAccessor;

    public DictionaryLinksService(LinkModel linkModel, ApiService ApiService, LocalStorageAccessor LocalStorageAccessor)
    {
        _linkModel = linkModel;
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
        string link = "http://";
        string[] patternParts = pattern.Split(':');

        List<string> properties = new() { "FLangCode", "TLangCode", "FLangName", "TLangName", "Word" };

        _linkModel.FLangName = LangCodesHelper.GetLanguageNameOrEmpty(fromLang); // Get LangName from dictionery names
        _linkModel.TLangName = LangCodesHelper.GetLanguageNameOrEmpty(toLang); // GetLangName

        _linkModel.Word = word;

        IrregularLink(pattern);

        foreach (var part in patternParts)
        {
            if (properties.Contains(part))
            {
                link += (string)_linkModel[part];
            }
            else
            {
                link += part;
            }
        }
        return link;
    }

    private void IrregularLink(string pattern)
    {
        if (_linkModel.FLangCode == "ar" || _linkModel.TLangCode == "ar")
        {
            if (pattern.Contains("arabdict.com"))
            {
                if (_linkModel.FLangCode == "ar")
                {
                    _linkModel.FLangName = LangCodesHelper.GetLanguageNameOrEmpty(_linkModel.TLangCode);
                    _linkModel.TLangName = "arabic";
                }
                if (_linkModel.FLangCode == "de" || _linkModel.TLangCode == "de")
                {
                    _linkModel.FLangName = "deutsch";
                    _linkModel.TLangName = "arabisch";
                }
            }

            if (pattern.Contains("almaany.com"))
            {
                if (_linkModel.FLangCode != "ar")
                {
                    _linkModel.TLangCode = _linkModel.FLangCode;
                }

            }
        }
    }
}
