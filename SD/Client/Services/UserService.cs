
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class UserService 
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {   
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _httpClient.GetFromJsonAsync<List<UserModel>>("api/User");
        }
        public async Task<IEnumerable<UserRelationshipsWithOneUser>> SearchUser(string CurrentUserId, string SearchText)
        {
            IEnumerable<UserRelationshipsWithOneUser> u = new List<UserRelationshipsWithOneUser>();

            u= await _httpClient.GetFromJsonAsync<IEnumerable<UserRelationshipsWithOneUser>>($"api/User/GetUsersByTextNew/{CurrentUserId}/{SearchText}");
            return u;
        }
        public async Task<Dictionary<string, string>> GetAllFriends(string id)
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/User/GetAllFriends/{id}");
        }
        public async Task<UserModel> GetUserById(string id)
        {
            return await _httpClient.GetFromJsonAsync<UserModel>($"api/User/GetUserById/{id}");
        }

        public async Task<Dictionary<string, string>> GetFriendRequestsById(string id)
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/User/GetFriendRequestsById/{id}");
        }
        public async Task<HttpResponseMessage> AddUser(UserModel user)
        {
                return await _httpClient.PostAsJsonAsync("api/User/Create", user);  
        }

        public async Task<HttpResponseMessage> RemoveUser(string id)
        {
            return await _httpClient.DeleteAsync($"api/User/?id={id}");
        }

        public async Task<HttpResponseMessage> UpdateUser(string id, UserModel newUser)
        {
            return await _httpClient.PutAsJsonAsync($"api/User/UpdateUser/{id}", newUser);
        }

        public async Task<HttpResponseMessage> AddFriendRequest(string userId, string friendId)
        {
            return await _httpClient.PostAsync($"api/User/AddFriendRequest/{userId}/{friendId}",null);
        }
        public async Task<HttpResponseMessage> RemoveFriendRequest(string userId, string friendId)
        {
            return await _httpClient.PostAsync($"api/User/RemoveFriendRequest/{userId}/{friendId}", null);
        }
        public async Task<HttpResponseMessage> AddFriend(string userId, string friendId)
        {
            return await _httpClient.PostAsync($"api/User/AddFriend/{userId}/{friendId}", null);
        }
        public async Task<HttpResponseMessage> RemoveFriend(string userId, string friendId)
        {
            return await _httpClient.PostAsync($"api/User/RemoveFriend/{userId}/{friendId}", null);
        }
    }
}