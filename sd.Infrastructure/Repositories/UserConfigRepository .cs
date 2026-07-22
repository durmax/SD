using System.Linq.Expressions;
using MongoDB.Driver;
using sd.Infrastructure.Models;
using sd.Application.Interfaces.Repositories;
using sd.Shared;

namespace sd.Infrastructure.Repositories
{
    public class UserConfigRepository : IUserConfigRepository
    {
        private readonly MongodbContext _context = null;

        public UserConfigRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<bool> Update(UserConfigModel userConfigModel)
        {
            var userConfigs = await _context.UsersConfigs.FindOneAndReplaceAsync(Builders<UserConfigModel>.Filter.Eq("UserId", userConfigModel.UserId), userConfigModel);
            return userConfigs != null ? true : false;
        }

        public async Task<IEnumerable<UserConfigModel>> GetByCondition(Expression<Func<UserConfigModel, bool>> expression)
        {
            return await _context.UsersConfigs.Find(expression).ToListAsync();
        }

        public async Task<UserConfigModel> GetById(string id)
        {
            var cursor = _context.UsersConfigs.Find(x => x.UserId == id);
            var res = await cursor.FirstOrDefaultAsync();
            return res;
        }
    }
}