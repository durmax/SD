using sd.Api.Infrastructure.Repositories;
using sd.Shared;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IUserConfigService
    {
        Task<UserConfigModel> GetById(string id);
        Task<bool> Update(UserConfigModel entity);

        Task<bool> AddUserLabel(string userId, string label);
        Task<bool> RemoveUserLabel(string userId, string label);

    }
    public class UserConfigService : IUserConfigService
    {
        private readonly IUserConfigRepository _userConfigRepository;

        public UserConfigService(IUserConfigRepository userConfigRepository)
        {
            _userConfigRepository = userConfigRepository;
        }

        public async Task<bool> Update(UserConfigModel entity)
        {
            return await _userConfigRepository.Update(entity);
        }

        public async Task<bool> AddUserLabel(string userId, string label)
        {
            var userConfig = await _userConfigRepository.GetById(userId);
            if (userConfig == null) return false;

            var labels = ListStringConverter.ToList(userConfig.Labels);

            if (!labels.Contains(label))
            {
                labels.Add(label);
                userConfig.Labels = ListStringConverter.ToString(labels);
                return await _userConfigRepository.Update(userConfig);
            }

            return false; // Label already exists
        }

        public async Task<bool> RemoveUserLabel(string userId, string label)
        {
            var userConfig = await _userConfigRepository.GetById(userId);
            if (userConfig is null) return false;

            var labels = ListStringConverter.ToList(userConfig.Labels);

            if (labels.Contains(label))
            {
                labels.Remove(label);
                userConfig.Labels = ListStringConverter.ToString(labels);
                return await _userConfigRepository.Update(userConfig);
            }

            return false; // Label was not found
        }

        public async Task<UserConfigModel> GetById(string id)
        {
            return await _userConfigRepository.GetById(id);
        }
    }
}
