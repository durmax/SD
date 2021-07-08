using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;
using sd.Api.Interfaces;

namespace sd.Api.Repositories
{
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
    }
}


