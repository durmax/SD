using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Services
{
    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;

        public bool IsAuthenticated { get; set; }
        public HttpClient HttpClient { get; set; }

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider, IHttpClientFactory httpClientFactory)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _httpClientFactory = httpClientFactory;

            HandleAuthenticationStateChanged(_authenticationStateProvider.GetAuthenticationStateAsync());

            // Subscribe to authentication state changes
            _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        }

        private void CreateHttpClient(bool isAuthenticated)
        {
            string httpClientName = isAuthenticated ? "forAuthenticatedUser" : "forNotAuthenticatedUser";
            HttpClient = _httpClientFactory.CreateClient(httpClientName);
        }

        private async void HandleAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            AuthenticationState authState = await task;

            IsAuthenticated = authState.User.Identity.IsAuthenticated;

            CreateHttpClient(IsAuthenticated);
        }
    }
}
