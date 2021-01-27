using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class WordService
    {
        private readonly HttpClient _httpClient;

        public WordService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> AddWord(WordModel word)
        {
            return await _httpClient.PostAsJsonAsync("api/Word/AddWord", word);
        }

        public async Task<Tuple<int, List<WordModel>>> GetWords(string currUsrId, string lang, int pageSize, int currentPage)
        {
            //await GetCurrentUserId();
            try
            {
                return await _httpClient.GetFromJsonAsync<Tuple<int, List<WordModel>>>($"api/Word/GetWords/{currUsrId}/{lang}/{pageSize}/{currentPage}");
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<WordModel>> GetAllWords(string CurrentUserId, string userId, int pageSize, int currentPage)
        {
            return await _httpClient.GetFromJsonAsync<List<WordModel>>($"api/Word/GetAllWords/{CurrentUserId}/{userId}/{pageSize}/{currentPage}");
        }

        public async Task<WordModel> GetWordById(string id)
        {
            return await _httpClient.GetFromJsonAsync<WordModel>($"api/Word/GetWord/{id}");
        }

        public async Task<WordModel> GetWordByText(string userId, string title)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<WordModel>($"api/Word/GetWordByText/{userId}/{title}");
            }
            catch
            {
                return new WordModel();
            }
        }

        public async Task<HttpResponseMessage> RemoveWord(string id)
        {
            return await _httpClient.DeleteAsync($"api/Word/DeleteWord/{id}");
        }

        public async Task<HttpResponseMessage> UpdateWord(WordModel newWord)
        {
            return await _httpClient.PutAsJsonAsync($"api/Word/UpdateWord", newWord);
        }

        public async Task<int> Like(string userId, string wordId)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/Word/Like/{userId}/{wordId}", null);
        }
        public async Task<Dictionary<string, Tuple<string, string>>> GetLikedUsers(string userId, string wordId)
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, Tuple<string, string>>>($"api/Word/GetLikedUsers/{userId}/{wordId}");
        }
    }
}
