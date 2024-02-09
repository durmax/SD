using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Services
{
    public class AuthenticationService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IServiceProvider service;

        public AuthenticationService(AuthenticationStateProvider authenticationStateProvider, IServiceProvider service)
        {
            _authenticationStateProvider = authenticationStateProvider;
            this.service = service;
        }

        public async Task<string> GetAuthenticatedUserName()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (await IsAuthenticated())
            {
                return user.Identity.Name;
            }
            else
            {
                return "Unknown";
            }
        }

        public async Task<bool> IsAuthenticated()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.Identity.IsAuthenticated;
        }

        public HttpClient AddHttpClient()
        {
            var hh= service.GetRequiredService<IHttpClientFactory>().CreateClient("SD.Client2.ServerAPI");
            return hh;
        }

    }

}
