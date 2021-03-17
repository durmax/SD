using SD.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class CommentService
    {
        private readonly HttpClient _httpClient;

        public CommentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SaveComment(CommentModel comment, string WordId)
        {
            return await _httpClient.PostAsJsonAsync($"api/Comment/SaveComment/{WordId}", comment);
        }

        public async Task<HttpResponseMessage> RemoveComment(string currUsr, string wordId, string commentId)
        {
            if (string.IsNullOrEmpty(wordId) || string.IsNullOrEmpty(commentId))
            {
                return null;
            }
            return await _httpClient.DeleteAsync($"api/Comment/DeleteComment/{currUsr}/{wordId}/{commentId}");
        }

        public async Task<IEnumerable<CommentModel>> GetWordComments(string wordId)
        {
            if (string.IsNullOrEmpty(wordId))
            {
                return null;
            }
            return await _httpClient.GetFromJsonAsync<IEnumerable<CommentModel>>($"api/Comment/GetWordComments/{wordId}");
        }
    }
}
