using Microsoft.AspNetCore.Components.Authorization;
using SD.Client.Pages;
using SD.Shared;
using System.Net.Http;
using System.Net.Http.Json;
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
                _currentUser.id = authState.User.FindFirst(c => c.Type == "oid")?.Value;
                _currentUser.name = authState.User.Identity.Name;
                _currentUser.email = authState.User.FindFirst(c => c.Type == "email")?.Value;
            }

            await AddUserAsync(_currentUser.id, _currentUser.email, _currentUser.name);

            _currentUser.isAuthTested = true;
        }
        private async Task AddUserAsync(string currUserId, string email, string name)
        {
            if (!string.IsNullOrWhiteSpace(currUserId) && !string.IsNullOrWhiteSpace(email))
            {
                UserModel userModel = new()
                {
                    UserId = currUserId,
                    Email = email,
                    Name = name
                };

                await _httpClient.PostAsJsonAsync("api/User/Create", userModel);
            }
        }
    }
}
