using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SD.Client.Services
{

    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly CurrentUser _currentUser;
        private readonly IHttpClientFactory _httpClientFactory;

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider,
               CurrentUser currentUser, IHttpClientFactory httpClientFactory)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _currentUser = currentUser;
            _httpClientFactory = httpClientFactory;

            HandleAuthenticationStateChanged(_authenticationStateProvider.GetAuthenticationStateAsync());

            // Subscribe to authentication state changes
            _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        }

        private void CreateHttpClient(bool isAuthenticated)
        {
            string httpClientName = isAuthenticated ? "forAuthenticatedUser" : "forNotAuthenticatedUser";
            _currentUser.httpClient = _httpClientFactory.CreateClient(httpClientName);
        }

        private async void HandleAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            AuthenticationState authState = await task;

            _currentUser.isAuthenticated = authState.User.Identity.IsAuthenticated;

            CreateHttpClient(_currentUser.isAuthenticated);

            if (_currentUser.isAuthenticated)
            {
                _currentUser.name = authState.User.Identity.Name;

                var response = await _currentUser.httpClient.GetAsync("api/User/Create");
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
        }
    }
}
