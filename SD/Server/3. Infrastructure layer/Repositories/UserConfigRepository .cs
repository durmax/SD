using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserConfigRepository
    {
        Task<UserConfigModel> GetUserConfigs(string userId);
        Task<bool> SetUserConfigs(UserConfigModel userConfigModel);
        Task<IEnumerable<string>> GetUserLabels(string userId);
        Task<bool> AddUserLabel(string userId, string label);
        Task<bool> RemoveUserLabel(string userId, string label);

    }
    public class UserConfigRepository : IUserConfigRepository
    {
        private readonly MongodbContext _context = null;

        public UserConfigRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<UserConfigModel> GetUserConfigs(string userId)
        {
            return await _context.UsersConfigs.Find(u => u.UserId.Equals(userId)).FirstOrDefaultAsync();
        }

        public async Task<bool> SetUserConfigs(UserConfigModel userConfigModel)
        {
            var userConfigs = await _context.UsersConfigs.FindOneAndReplaceAsync(Builders<UserConfigModel>.Filter.Eq("UserId", userConfigModel.UserId), userConfigModel);
            return userConfigs != null ? true : false;
        }
        public async Task<IEnumerable<string>> GetUserLabels(string userId)
        {
            var userConfigModel = await GetUserConfigs(userId);
            return ListStringConverter.ToList(userConfigModel.Labels);
        }

        public async Task<bool> AddUserLabel(string userId, string label)
        {
            var userConfigs = await GetUserConfigs(userId);

            if (userConfigs != null)
            {
                userConfigs.Labels += "," + label;
            }
            else
            {
                return false;
            }
            return await SetUserConfigs(userConfigs);
        }

        public async Task<bool> RemoveUserLabel(string userId, string label)
        {
            var userConfigs = await GetUserConfigs(userId);

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
            return await SetUserConfigs(userConfigs);
        }
    }
}


