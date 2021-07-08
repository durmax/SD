using sd.Api.Interfaces;
using SD.Shared;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class UserConfigService
    {
        private readonly IUserConfigRepository _userConfigRepository;
        public UserConfigService(IUserConfigRepository userConfigRepository)
        {
            _userConfigRepository = userConfigRepository;
        }

        public async Task<UserConfigModel> GetUserConfigs(string userId)
        {
            return await _userConfigRepository.GetUserConfigs(userId);
        }

        public async Task<bool> SetUserConfigs(UserConfigModel userConfigModel)
        {
          return await  _userConfigRepository.SetUserConfigs(userConfigModel);
        }
    }
}
