using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SD.Client.Models;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class OtherPageService
    {
        private readonly CurrentUserService _currentUser;
        private readonly UriService _uriService;
        private readonly LinkParam _reqLinkP;
        private readonly LoggingService _logger;

        public OtherPageService(CurrentUserService currentUser, UriService uriService, LinkParam reqLinkP, LoggingService logger)
        {
            _currentUser = currentUser;
            _uriService = uriService;
            _reqLinkP = reqLinkP;
            _logger = logger;
        }

        public async Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {

            List<OtherPageResModel> OPResModels = null;
            
            try
            {
                _logger.Log(this.ToString(), LogLevel.Information, "before GetOPResModels call url:" + $"api/OtherPage/{fromLang}/{toLang}");
                OPResModels = await _currentUser.HttpClient.GetFromJsonAsync<List<OtherPageResModel>>($"api/OtherPage/{fromLang}/{toLang}");

                var serializedOtherPageModels = JsonConvert.SerializeObject(OPResModels);
                _logger.Log(this.ToString(), LogLevel.Information, "after GetOPResModels " + serializedOtherPageModels);
            }
            catch(Exception  ex)    
            {
                _logger.Log(this.ToString(), LogLevel.Error, "after GetOPResModels 1 " + ex.Message);
                _logger.Log(this.ToString(), LogLevel.Error, "after GetOPResModels 2 " + ex.ToString());
                _logger.Log(this.ToString(), LogLevel.Error, "after GetOPResModels 3 " + _currentUser.ToString());
                _logger.Log(this.ToString(), LogLevel.Error, "after GetOPResModels 3 " + _currentUser?.HttpClient?.ToString());
                _logger.Log(this.ToString(), LogLevel.Error, "after GetOPResModels 3 " + _currentUser?.HttpClient?.BaseAddress);
            }
            return OPResModels;
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

        public async Task<OtherPageModel> GetOtherPageById(string id)
        {
            return await _currentUser.HttpClient.GetFromJsonAsync<OtherPageModel>($"api/OtherPage/{id}");
        }

        public async Task<HttpResponseMessage> RegisterOtherPage(OtherPageModel otherPage)
        {
            return await _currentUser.HttpClient.PostAsJsonAsync("api/OtherPage", otherPage);
        }

        public async Task<HttpResponseMessage> RemoveOtherPage(string id)
        {
            return await _currentUser.HttpClient.DeleteAsync($"api/OtherPage/?id={id}");
        }

        public async Task<HttpResponseMessage> UpdateOtherPage(OtherPageModel newOtherPage)
        {
            return await _currentUser.HttpClient.PutAsJsonAsync("api/OtherPage", newOtherPage);
        }
    }
}
