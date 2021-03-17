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

        public async Task<HttpResponseMessage> AddWord(WordDto word)
        {
            return await _httpClient.PostAsJsonAsync("api/Word/AddWord", word);
        }

        //public async Task<Tuple<int, List<WordModel>>> GetWords(string currUsrId, string lang, int pageSize, int currentPage)
        //{
        //    //await GetCurrentUserId();
        //    try
        //    {
        //        return await _httpClient.GetFromJsonAsync<Tuple<int, List<WordModel>>>($"api/Word/GetWords/{currUsrId}/{lang}/{pageSize}/{currentPage}");
        //    }
        //    catch
        //    {
        //        return null;
        //    }

        //}

        public async Task<Tuple<int, List<WordDto>>> GetPageWordsFromUserID(string CurrentUserId, string userId, int pageSize, int currentPage)
        {
            if (userId==null)
            {
                userId = CurrentUserId;
            }
            return await _httpClient.GetFromJsonAsync<Tuple<int, List<WordDto>>>($"api/Word/GetPageWordsFromUserID/{CurrentUserId}/{userId}/{pageSize}/{currentPage}");
        }

        public async Task<Tuple<int, List<WordDto>>> GetPageWordsFromAllUseres(string CurrentUserId, int pageSize, int currentPage)
        {
            CurrentUserId ??= "0";
            return await _httpClient.GetFromJsonAsync<Tuple<int, List<WordDto>>>($"api/Word/GetPageWordsFromAllUseres/{CurrentUserId}/{pageSize}/{currentPage}");
        }

        public async Task<WordDto> GetWordById(string id)
        {
            return await _httpClient.GetFromJsonAsync<WordDto>($"api/Word/GetWord/{id}");
        }

        public async Task<WordDto> GetWordByText(string userId, string title)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<WordDto>($"api/Word/GetWordByText/{userId}/{title}");
            }
            catch
            {
                return new WordDto();
            }
        }

        public async Task<HttpResponseMessage> RemoveWord(string id)
        {
            return await _httpClient.DeleteAsync($"api/Word/DeleteWord/{id}");
        }

        public async Task<HttpResponseMessage> UpdateWord(WordDto newWord)
        {
            return await _httpClient.PutAsJsonAsync($"api/Word/UpdateWord", newWord);
        }

        public async Task<int> Like(string userId, string wordId)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/Word/Like/{userId}/{wordId}", null);
        }

        public async Task<IEnumerable<UserRelationshipsWithOneUserDto>> GetLikedUsers(string userId, string wordId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/Word/GetLikedUsers/{userId}/{wordId}");
        }
    }
}
