using SD.Client.Models;
using System.Collections.Generic;

namespace SD.Client.Services
{
    public class UriService
    {
        private LinkModel _linkModel;
        private LangCodeService _langCodeService;

        public UriService(LinkModel linkModel, LangCodeService langCodeService)
        {
            _linkModel = linkModel;
            _langCodeService = langCodeService;
        }

        public string UriBuild(LinkParam linkParam)
        {
            string link = "https://";

            string pattern = linkParam.Pattern;
            string[] patternParts = pattern.Split(':');

            List<string> properties = new List<string> { "FLangCode", "TLangCode", "FLangName", "TLangName", "Word" };

            _linkModel.FLangCode = linkParam.FLangCode;
            _linkModel.TLangCode = linkParam.TLangCode;

            _linkModel.FLangName = _langCodeService.Langs[linkParam.FLangCode]; // Get LangName from dictionery names
            _linkModel.TLangName = _langCodeService.Langs[linkParam.TLangCode];     // GetLangName


            _linkModel.Word = linkParam.Word;

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
                        _linkModel.FLangName = _langCodeService.Langs[_linkModel.TLangCode];
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
}
