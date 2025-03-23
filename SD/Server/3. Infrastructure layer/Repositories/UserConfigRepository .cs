using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserConfigRepository: ICrudBase<UserConfigModel>
    {
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

        public async Task<bool> Update(UserConfigModel userConfigModel)
        {
            var userConfigs = await _context.UsersConfigs.FindOneAndReplaceAsync(Builders<UserConfigModel>.Filter.Eq("UserId", userConfigModel.UserId), userConfigModel);
            return userConfigs != null ? true : false;
        }
        public async Task<IEnumerable<string>> GetUserLabels(string userId)
        {
            var userConfigModels = await GetByCondation(u => u.UserId == userId);
            return ListStringConverter.ToList(userConfigModels.First().Labels);
        }

        public async Task<bool> AddUserLabel(string userId, string label)
        {
            var userConfigs = await GetByCondation(u => u.UserId == userId);

            if (userConfigs.First() != null)
            {
                userConfigs.First().Labels += "," + label;
            }
            else
            {
                return false;
            }
            return await Update(userConfigs.First());
        }

        public async Task<bool> RemoveUserLabel(string userId, string label)
        {
            var userConfigs = await GetByCondation(u => u.UserId == userId);

            List<string> labels = ListStringConverter.ToList(userConfigs.First().Labels);

            if (userConfigs != null)
            {
                labels.Remove(label);
                userConfigs.First().Labels = ListStringConverter.ToString(labels);
            }
            else
            {
                return false;
            }
            return await Update(userConfigs.First());
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