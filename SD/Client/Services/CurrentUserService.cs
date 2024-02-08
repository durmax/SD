using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{

    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly CurrentUser _currentUser;
        private readonly HttpClient _httpClient;

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider,
               CurrentUser currentUser, HttpClient httpClient)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _currentUser = currentUser;
            _httpClient = httpClient;
        }

        public async Task GetAuth()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _currentUser.isAuthenticated = authState.User.Identity.IsAuthenticated;
            if (_currentUser.isAuthenticated)
            {
                //var id = authState.User.FindFirst(c => c.Type == "oid")?.Value;
                _currentUser.name = authState.User.Identity.Name;
                _currentUser.email = authState.User.FindFirst(c => c.Type == "email")?.Value;

                if (_currentUser.email != null)
                {
                    var response = await AddUserAsync(_currentUser.email, _currentUser.name);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseBody))
                        {
                            var respondedUser = JsonSerializer.Deserialize<UserModel>(responseBody);

                            if (respondedUser != null)
                            {
                                _currentUser.id = respondedUser.UserId;
                                _currentUser.email = respondedUser.Email;
                                _currentUser.name = respondedUser.Name;
                            }
                        }
                    }
                }
                _currentUser.isAuthTested = true;
            }
        }
        private async Task<HttpResponseMessage> AddUserAsync(string email, string name)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                UserModel userModel = new()
                {
                    Email = email,
                    Name = name
                };

                return await _httpClient.PostAsJsonAsync<UserModel>("api/User/Create", userModel);
            }
            return new HttpResponseMessage();
        }
    }
}
