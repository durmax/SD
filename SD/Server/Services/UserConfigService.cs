using sd.Api.Interfaces;
using SD.Shared;
using System.Collections.Generic;
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
            return await _userConfigRepository.SetUserConfigs(userConfigModel);
        }

        public async Task<IEnumerable<string>> GetUserLabels(string userId)
        {
            var userConfigModel = await _userConfigRepository.GetUserConfigs(userId);
            return ListStringConverter.ToList(userConfigModel.Labels);
        }

        public async Task<bool> AddUserLabel(string userId, string label)
        {
            var userConfigs = await _userConfigRepository.GetUserConfigs(userId);

            if (userConfigs != null)
            {
                userConfigs.Labels += "," + label;
            }
            else
            {
                return false;
            }
            return await _userConfigRepository.SetUserConfigs(userConfigs);
        }

        public async Task<bool> RemoveUserLabel(string userId, string label)
        {
            var userConfigs = await _userConfigRepository.GetUserConfigs(userId);

            List<string> labels = ListStringConverter.ToList(userConfigs.Labels);

            if (userConfigs != null)
            {
                labels.Remove(label);
                userConfigs.Labels = ListStringConverter.ToString(labels);
            }
            else
            {
                return false;
            }
            return await _userConfigRepository.SetUserConfigs(userConfigs);
        }
    }
}
