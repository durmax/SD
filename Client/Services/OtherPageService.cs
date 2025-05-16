using sd.Client.Models;
using sd.Shared;
using System.Collections.Generic;

namespace sd.Client.Services
{
    public class OtherPageService
    {
        private readonly UriService _uriService;
        private readonly LinkParam _reqLinkP;

        public OtherPageService(UriService uriService, LinkParam reqLinkP)
        {
            _uriService = uriService;
            _reqLinkP = reqLinkP;
        }

        public IEnumerable<OtherPageResModel> MakeLinks(IEnumerable<OtherPageResModel> OtherPageModels, string word, string fromLang, string toLang)
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
    }
}
