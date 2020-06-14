using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<WordModel>> GetAllWords(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<WordModel>>($"api/Word/GetAllWords/{userId}");
        }

        public async Task<WordModel> GetWordById(string id)
        {
            return await _httpClient.GetFromJsonAsync<WordModel>($"api/Word/GetWord/{id}");
        }

        public async Task<WordModel> GetWordByText(string userId, string text)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> RemoveWord(string id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<WordModel> UpdateWord(string id, WordModel newWord)
        {
            throw new System.NotImplementedException();
        }
    }
}
