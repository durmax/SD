using sd.Application.Interfaces.Repositories;
using sd.Application.Services.Gemini;
using sd.Shared;
using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;

namespace sd.Application.Services;

public interface IOtherPageService
{
    Task<OtherPageModel?> GetById(string id);
    Task<IEnumerable<OtherPageModel>> GetByCondition(Expression<Func<OtherPageModel, bool>> expression);
    Task<bool> Create(OtherPageModel entity);
    Task<bool> Update(OtherPageModel entity);
    Task<bool> Delete(string id);

    Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
    Task<OtherPageModel> GetModelByAI(string exampleURL, CancellationToken cancellationToken);
}

public class OtherPageService : IOtherPageService
{
    private readonly IOtherPageRepository _otherPageRepository;
    private readonly GeminiService _geminiService;

    public OtherPageService(IOtherPageRepository otherPageRepository, GeminiService geminiService)
    {
        _otherPageRepository = otherPageRepository;
        _geminiService = geminiService;
    }

    public async Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
    {
        List<OtherPageResModel> res = new List<OtherPageResModel>();


        var otherPages = await _otherPageRepository.GetByCondition(o =>
              (o.PrimLangs == "All" && o.SecLangs == "All")
           || (o.PrimLangs.Contains(fromLang) && (o.SecLangs.Contains(toLang) || o.SecLangs == "All"))
           || (o.PrimLangs.Contains(toLang) && o.SecLangs.Contains(fromLang))
               );

        if (otherPages != null)
        {
            int i = 0;
            foreach (var otherPage in otherPages)
            {
                if (!string.IsNullOrEmpty(otherPage.Pattern))
                {
                    OtherPageResModel otherPageResModel = new OtherPageResModel()
                    {
                        Pattern = otherPage.Pattern,
                        Host = otherPage.Host,
                        Type = otherPage.PageType,
                        Eval = otherPage.Eval == 0 ? i++ : otherPage.Eval
                    };
                    res.Add(otherPageResModel);
                }
            }
            // return res;
        }
        return res.OrderBy(o => o.Eval).ToList();
    }

    public async Task<IEnumerable<OtherPageModel>> GetByCondition(Expression<Func<OtherPageModel, bool>> expression)
    {
        return await _otherPageRepository.GetByCondition(expression);
    }

    public Task<bool> Create(OtherPageModel entity)
    {
        return _otherPageRepository.Create(entity);
    }

    public Task<bool> Update(OtherPageModel entity)
    {
        return _otherPageRepository.Update(entity);
    }

    public Task<bool> Delete(string id)
    {
        return _otherPageRepository.Delete(id);
    }

    public async Task<OtherPageModel?> GetById(string id)
    {
        return await _otherPageRepository.GetById(id);
    }

    public async Task<OtherPageModel> GetModelByAI(string exampleURL, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(exampleURL)) return null;

        string url = exampleURL;
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }
        var host = new Uri(url).Authority;
        if (host.StartsWith("www."))
            host = host.Substring(4);

        var model = await GetByCondition(x => x.Host == host);
        if (model.Any())
        {
            return model.FirstOrDefault();
        }

        var prompt =@$""" 
I provide you URL, you return me a json.

Example1 with LangName:
URL: https://context.reverso.net/translation/german-arabic/Test
Expected return json is:
{{\""Host\"": \""context.reverso.net\"", \""PrimLangs\"": \""de\"", \""SecLangs\"": \""ar\"", \""Pattern\"": \""context.reverso.net/translation/:FLangName:-:TLangName:/:Word:\""}}

Example2 with LangCode:
URL: www.deepl.com/translator#de/en/Test
Expected return json is:
{{\""Host\"": \""deepl.com\"", \""PrimLangs\"": \""de\"", \""SecLangs\"": \""en\"", \""Pattern\"": \""deepl.com/translator#:FlangCode:/:TLangCode:/:Word:\""}}

Return me new json of the new URL: {exampleURL} ""

The return value must be without any char extra in this form:
{{Host: stringValue, PrimLangs: stringValue, SecLangs: stringValue, Pattern: stringValue}}
";

        var aiRes = await _geminiService.ProcessStringAsync(prompt, cancellationToken);
        aiRes = aiRes.Replace("json", string.Empty);
        aiRes = aiRes.Replace("```", string.Empty);

        var res = JsonSerializer.Deserialize<OtherPageModel>(aiRes.TrimEnd());

        if (res.Pattern.StartsWith("www."))
            res.Pattern = res.Pattern.Substring(4);

        if (res.Host.StartsWith("www."))
            res.Host = res.Pattern.Substring(4);

        // check if pattern or www.pattern exists
        model = await GetByCondition(
    x => x.Pattern == res.Pattern ||
    x.Pattern == "www." + res.Pattern);

        if (model.Any())
        {
            return model.FirstOrDefault();
        }

        res.PageType = "Dict";
        res.Eval = 0;
        res.Notes = new List<string> { "Generated by Gemini AI, Created by example URL: " + exampleURL };

        return res;
    }
}
