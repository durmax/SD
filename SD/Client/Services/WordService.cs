using SD.Shared;
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

        public async Task<List<WordModel>> GetAllWords(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<WordModel>>($"api/Word/GetAllWords/{userId}");
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
    }
}
