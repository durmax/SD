using sd.Api.Infrastructure.Repositories;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IUserConfigService : ICrudBase<UserConfigModel>
    {
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
            var userConfigs = await _userConfigRepository.GetByCondation(u => u.UserId == userId);
            if (!userConfigs.Any()) return false;

            var userConfig = userConfigs.First();
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
            var userConfigs = await _userConfigRepository.GetByCondation(u => u.UserId == userId);
            if (!userConfigs.Any()) return false;

            var userConfig = userConfigs.First();
            var labels = ListStringConverter.ToList(userConfig.Labels);

            if (labels.Contains(label))
            {
                labels.Remove(label);
                userConfig.Labels = ListStringConverter.ToString(labels);
                return await _userConfigRepository.Update(userConfig);
            }

            return false; // Label was not found
        }

        public Task<IEnumerable<UserConfigModel>> GetByCondation(Expression<Func<UserConfigModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Create(UserConfigModel entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(string id)
        {
            throw new NotImplementedException();
        }
    }
}
