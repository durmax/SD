using SD.Client.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class WordService
    {
        private readonly CurrentUser _currentUser;

        public WordService(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public async Task<HttpResponseMessage> AddWord(WordDto word)
        {
            return await _currentUser.httpClient.PostAsJsonAsync("api/Word", word);
        }

        public async Task<List<WordDto>> GetPageWords(string currentUserId, string userId, int pageSize, int currentPage)
        {
            return await _currentUser.httpClient.GetFromJsonAsync<List<WordDto>>($"api/Word/GetPageWords/{currentUserId ?? "0"}/{userId ?? "0"}/{pageSize}/{currentPage}");
        }
        
        public async Task<WordDto> GetWordById(string id)
        {
            return await _currentUser.httpClient.GetFromJsonAsync<WordDto>($"api/Word/{id}");
        }

        public async Task<List<string>> GetWordsContainText(string userId, string title)
        {
            try
            {
                var w = await _currentUser.httpClient.GetFromJsonAsync<List<string>>($"api/Word/GetWordsContainText/{userId}/{title}");
                return w;
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> RemoveWord(string id)
        {
            return await _currentUser.httpClient.DeleteAsync($"api/Word/{id}");
        }

        public async Task<HttpResponseMessage> UpdateWord(WordDto newWord)
        {
            return await _currentUser.httpClient.PutAsJsonAsync($"api/Word", newWord);
        }

        public async Task<int> Like(string userId, string wordId)
        {
            return await _currentUser.httpClient.GetFromJsonAsync<int>($"api/Word/Like/{userId}/{wordId}");
        }

        public async Task<IEnumerable<UserRelationshipsWithOneUserDto>> GetLikedUsers(string userId, string wordId)
        {
            userId ??= "0";
            return await _currentUser.httpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/Word/GetLikedUsers/{userId}/{wordId}");
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
                try
                {
                    var response = await _currentUser.httpClient.GetFromJsonAsync<LanguageToolResponse>($"https://api.languagetool.org/v2/check?language={wordLang}&text={str}");

                    // Extract the list of string values from Matches.Replacements.Value
                    return response.Matches
                        .SelectMany(match => match.Replacements)
                        .Select(replacement => replacement.Value) //.Where(value =>  value.ToLower() != str.ToLower())
                        .Take(15)
                        .ToList();
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }
    }
}
