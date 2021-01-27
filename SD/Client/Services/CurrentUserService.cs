using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace SD.Client.Services
{

    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> IsAuth()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity.IsAuthenticated;
        }

        public async Task<string> GetCurrUsrId()
        {
            string id = "0";

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                id = authState.User.FindFirst(c => c.Type == "oid")?.Value;
            }
            return id;
        }

        public async Task<string> GetCurrUsrName()
        {
            string name = "";
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                name = authState.User.Identity.Name;
            }
            return name;
        }

        public async Task<string> GetCurrUsrEmail()
        {
            string email = "";
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                email = authState.User.FindFirst(c => c.Type == "email")?.Value;
            }
            return email;
        }
    }
}
