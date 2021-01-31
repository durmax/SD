using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class RelationshipService
    {
        private readonly HttpClient _httpClient;

        public RelationshipService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetRelationshipById(string id)
        {
            return await _httpClient.PostAsync($"api/Relationship/GetRelationshipById/{id}", null);
        }
        public async Task<Dictionary<string, string>> GetAllFriends(string id)
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Relationship/GetAllFriends/{id}");
        }

        public async Task<Dictionary<string, string>> GetFriendRequestsById(string id)
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Relationship/GetFriendRequestsById/{id}");
        }

        public async Task<HttpResponseMessage> RemoveRelationship(string id)
        {
            return await _httpClient.DeleteAsync($"api/Relationship/RemoveRelationship/{id}");
        }
        public async Task<HttpResponseMessage> RemoveFriendship(string UserId, Reletion reletion, string friendId)
        {
            return await _httpClient.DeleteAsync($"api/Relationship/RemoveFriendship/{UserId}/{reletion}/{friendId}");
        }

        public async Task<HttpResponseMessage> AddRelationship(RelationshipModel relationship)
        {
            return await _httpClient.PostAsJsonAsync($"api/Relationship/AddRelationship", relationship);
        }

        //public async Task<HttpResponseMessage> AddFriendRequest(string userId, string friendId)
        //{
        //    return await _httpClient.PostAsync($"api/Relationship/AddFriendRequest/{userId}/{friendId}", null);
        //}

        //public async Task<HttpResponseMessage> RemoveFriendRequest(string userId, string friendId)
        //{
        //    return await _httpClient.PostAsync($"api/Relationship/RemoveFriendRequest/{userId}/{friendId}", null);
        //}
        //public async Task<HttpResponseMessage> AddFriend(string userId, string friendId)
        //{
        //    return await _httpClient.PostAsync($"api/Relationship/AddFriend/{userId}/{friendId}", null);
        //}
        //public async Task<HttpResponseMessage> RemoveFriend(string userId, string friendId)
        //{
        //    return await _httpClient.PostAsync($"api/Relationship/RemoveFriend/{userId}/{friendId}", null);
        //}
    }
}
