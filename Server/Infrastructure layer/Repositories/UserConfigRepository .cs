using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using sd.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserConfigRepository: ICrudBase<UserConfigModel>
    {
    }
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

        public async Task<IEnumerable<UserConfigModel>> GetByCondation(Expression<Func<UserConfigModel, bool>> expression)
        {
            return await _context.UsersConfigs.Find(expression).ToListAsync();
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