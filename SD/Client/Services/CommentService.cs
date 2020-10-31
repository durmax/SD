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
            return await _httpClient.PostAsJsonAsync($"api/Comment/AddComment/{WordId}", comment);
        }

        public async Task<CommentModel> GetComment(string CommentId)
        {
            CommentModel commentModel = new CommentModel();
             commentModel = await _httpClient.GetFromJsonAsync<CommentModel>($"api/Comment/GetComment/{CommentId}");
            return commentModel;
        }

        public async Task<List<CommentModel>> GetAllComments(string wordId)
        {
            return await _httpClient.GetFromJsonAsync<List<CommentModel>>($"api/Comment/GetAllComments/{wordId}");
        }

        public async Task<HttpResponseMessage> RemoveComment(string id)
        {
            return await _httpClient.DeleteAsync($"api/Comment/DeleteComment/{id}");
        }

        public async Task<HttpResponseMessage> UpdateComment(CommentModel commentModel)
        {
            return await _httpClient.PutAsJsonAsync($"api/Comment/UpdateComment", commentModel);
        }
    }
}
