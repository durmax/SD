
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
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
        public async Task<IEnumerable<UserModel>> SearchUser(string text)
        {
            var xxx= await _httpClient.GetFromJsonAsync<IEnumerable<UserModel>>($"api/User/GetUsersByText/{text}");
            return xxx;
        }
        public async Task<UserModel> GetUserById(string id)
        {
            return await _httpClient.GetFromJsonAsync<UserModel>($"api/User/GetUserById/{id}");
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

        //public async Task<HttpResponseMessage> GetUserByAccessTokenAsync(string token)
        //{  try
        //    {
        //        TransObj status = new TransObj();
        //        status.SetringVar = token;
        //        return await _httpClient.PostAsJsonAsync("api/User/ValidateUserByAccessToken", status);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

    }
}