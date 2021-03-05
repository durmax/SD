using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using System.Threading.Tasks;

namespace SD.Client.Services
{

    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserService _userService;

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider, UserService userService)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _userService = userService;
        }

        public string id;
        public string name;
        public string email;
        public bool isAuthenticated;
        public bool isAuthTested;

        public async Task GetAuth()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            isAuthenticated= authState.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                id = authState.User.FindFirst(c => c.Type == "oid")?.Value;
                name = authState.User.Identity.Name;
                email = authState.User.FindFirst(c => c.Type == "email")?.Value;
            }

            await AddUserAsync(id, email, name);

            isAuthTested = true;
        }
        private async Task AddUserAsync(string currUserId, string email, string name)
        {
            UserModel userModel = new UserModel();
            userModel.UserId = currUserId;
            userModel.Email = email;
            userModel.Name = name;
            await _userService.AddUser(userModel);
        }
    }
}
