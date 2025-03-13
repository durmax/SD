using SD.Client.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class WordService
    {
        private readonly ApiService _apiService;

        public WordService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<HttpResponseMessage> AddWord(WordDto word)
        {
            return await _apiService.PostAsync<HttpResponseMessage>("api/Word", word);
        }

        public async Task<List<WordDto>> GetPageWords(string userId, int pageSize, int currentPage)
        {
            return await _apiService.GetAsync<List<WordDto>>($"api/Word/GetPageWords/{userId}/{pageSize}/{currentPage}");
        }

        public async Task<WordDto> GetWordById(string id)
        {
            return await _apiService.GetAsync<WordDto>($"api/Word/{id}");
        }

        public async Task<List<string>> GetWordsContainText(string title)
        {
            return await _apiService.GetAsync<List<string>>($"api/Word/GetWordsContainText/{title}");
        }

        public async Task<HttpResponseMessage> RemoveWord(string id)
        {
            return await _apiService.DeleteAsync($"api/Word/{id}");
        }

        public async Task<HttpResponseMessage> UpdateWord(WordDto newWord)
        {
            return await _apiService.PutAsync<HttpResponseMessage>($"api/Word", newWord);
        }

        public async Task<int> Like(string wordId)
        {
            return await _apiService.GetAsync<int>($"api/Word/Like/{wordId}");
        }

        public async Task<IEnumerable<UserRelationshipsWithOneUserDto>> GetLikedUsers(string wordId)
        {
            return await _apiService.GetAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/Word/GetLikedUsers/{wordId}");
        }

        public async Task<List<string>> GetLanguageToolWords(string wordLang, string str)
        {
            wordLang = wordLang switch
            {
                "de" => "de-DE",
                "en" => "en-US",
                _ => string.Empty,
            };
            if (!string.IsNullOrEmpty(wordLang))
            {
                var response = await _apiService.GetAsync<LanguageToolResponse>($"https://api.languagetool.org/v2/check?language={wordLang}&text={str}");

                // Extract the list of string values from Matches.Replacements.Value
                return response.Matches
                    .SelectMany(match => match.Replacements)
                    .Select(replacement => replacement.Value) //.Where(value =>  value.ToLower() != str.ToLower())
                    .Take(15)
                    .ToList();
            }
            return null;
        }
    }
}
