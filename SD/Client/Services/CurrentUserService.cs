using Microsoft.AspNetCore.Components.Authorization;
using SD.Shared;
using System.Threading.Tasks;

namespace SD.Client.Services
{

    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserService _userService;
        private readonly CurrentUser _currentUser;

        public CurrentUserService(AuthenticationStateProvider authenticationStateProvider, 
            UserService userService, CurrentUser currentUser)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _userService = userService;
            _currentUser = currentUser;
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
            UserModel userModel = new UserModel();
            userModel.UserId = currUserId;
            userModel.Email = email;
            userModel.Name = name;
            await _userService.AddUser(userModel);
        }
    }
}
