using Microsoft.Extensions.Logging;
using SD.Client.Models;
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
        private readonly CurrentUserService _currentUser;
        private readonly LoggingService _logger;

        public WordService(CurrentUserService currentUser, LoggingService logger)
        {
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> AddWord(WordDto word)
        {
            try
            {
                return await _currentUser.HttpClient.PostAsJsonAsync("api/Word", word);
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
        }

        public async Task<List<WordDto>> GetPageWords(string userId, int pageSize, int currentPage)
        {
            try
            {
                return await _currentUser.HttpClient.GetFromJsonAsync<List<WordDto>>($"api/Word/GetPageWords/{userId ?? "0"}/{pageSize}/{currentPage}");
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
        }

        public async Task<WordDto> GetWordById(string id)
        {
            try
            {
                var res = await _currentUser.HttpClient.GetFromJsonAsync<WordDto>($"api/Word/{id}");
                return res;
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public async Task<List<string>> GetWordsContainText(string title)
        {
            try
            {
                return await _currentUser.HttpClient.GetFromJsonAsync<List<string>>($"api/Word/GetWordsContainText/{title}");
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public async Task<HttpResponseMessage> RemoveWord(string id)
        {
            try
            {
                return await _currentUser.HttpClient.DeleteAsync($"api/Word/{id}");
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
        }

        public async Task<HttpResponseMessage> UpdateWord(WordDto newWord)
        {
            try
            {
                return await _currentUser.HttpClient.PutAsJsonAsync($"api/Word", newWord);
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
        }

        public async Task<int> Like(string wordId)
        {
            try
            {
                return await _currentUser.HttpClient.GetFromJsonAsync<int>($"api/Word/Like/{wordId}");
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<UserRelationshipsWithOneUserDto>> GetLikedUsers(string wordId)
        {
            try
            {
                return await _currentUser.HttpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUserDto>>($"api/Word/GetLikedUsers/{wordId}");
            }
            catch (Exception ex)
            {
                _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                throw;
            }
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
                    var response = await _currentUser.HttpClient.GetFromJsonAsync<LanguageToolResponse>($"https://api.languagetool.org/v2/check?language={wordLang}&text={str}");

                    // Extract the list of string values from Matches.Replacements.Value
                    return response.Matches
                        .SelectMany(match => match.Replacements)
                        .Select(replacement => replacement.Value) //.Where(value =>  value.ToLower() != str.ToLower())
                        .Take(15)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.Log(this.ToString(), LogLevel.Error, ex.ToString());
                    return null;
                }

            }
            return null;
        }
    }
}
