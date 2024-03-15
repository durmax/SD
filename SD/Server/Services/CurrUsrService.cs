using SD.Shared;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class CurrUsrService
    {
        private readonly UserService _userService;

        public CurrUsrService(UserService userService)
        {
            _userService = userService;
        }

        public async Task<UserModel?> GetCurrentUser(ClaimsPrincipal user)
        {
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;

                return await _userService.RegisterUserAsync(email);
            }
            else return null;
        }
    }
}
