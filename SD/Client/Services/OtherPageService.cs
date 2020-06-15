using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SD.Client.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class OtherPageService
    {
        private readonly HttpClient _httpClient;
        private readonly UriService _uriService;
        private LinkParam _reqLinkP;

        public OtherPageService(HttpClient http,
                                   UriService uriService,
                                   LinkParam reqLinkP)
        {
            _httpClient = http;
            _uriService = uriService;
            _reqLinkP = reqLinkP;
        }

        public async Task<IEnumerable<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {

            List<OtherPageResModel> OPResModels = null;
            
            try
            {
                OPResModels = await _httpClient.GetFromJsonAsync<List<OtherPageResModel>>($"api/OtherPage/{fromLang}/{toLang}");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                //exception.RedirectToLogin();
            }
            return OPResModels;
        }

        public IEnumerable<OtherPageResModel> MakeLinks(IEnumerable<OtherPageResModel> OtherPageModels, string word, string fromLang, string toLang)
        {
            List<OtherPageResModel> res = new List<OtherPageResModel>();
            if (OtherPageModels != null)
            {

                foreach (var otherPage in OtherPageModels)
                {
                    if (!string.IsNullOrEmpty(otherPage.Pattern))
                    {
                        _reqLinkP.Pattern = otherPage.Pattern;
                        _reqLinkP.Word = word;
                        _reqLinkP.FLangCode = fromLang;
                        _reqLinkP.TLangCode = toLang;

                        string newLink = _uriService.UriBuild(_reqLinkP);


                            OtherPageResModel otherPageResModel = otherPage;
                            otherPageResModel.Link = newLink;
                            res.Add(otherPageResModel);
                        
                    }
                }
            }
            return res;
        }

        public async Task<OtherPageModel> GetOtherPageById(string id)
        {
            return await _httpClient.GetFromJsonAsync<OtherPageModel>($"api/OtherPage/GetById/{id}");
        }

        public async Task<HttpResponseMessage> RegisterOtherPage(OtherPageModel otherPage)
        {
            return await _httpClient.PostAsJsonAsync("api/OtherPage/Create", otherPage);
        }

        public async Task<HttpResponseMessage> RemoveOtherPage(string id)
        {
            return await _httpClient.DeleteAsync($"api/OtherPage/?id={id}");
        }

        public async Task<HttpResponseMessage> UpdateOtherPage(OtherPageModel newOtherPage)
        {
            return await _httpClient.PostAsJsonAsync("api/OtherPage/Update", newOtherPage);
        }
    }
}
