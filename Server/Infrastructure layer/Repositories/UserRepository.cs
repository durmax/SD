using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using sd.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression);
        Task<UserModel> GetById(string id);
        Task<bool> Create(UserModel user);
        Task<bool> Update(UserModel newVer);
        Task<bool> Delete(string id);
    }
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;

        public UserRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<bool> Create(UserModel user)
        {
           await _context.Users.InsertOneAsync(user);
           return true;
        }

        public async Task<bool> Update(UserModel newVer)
        {
            await _context.Users.FindOneAndReplaceAsync(Builders<UserModel>.Filter.Eq("UserId", newVer.UserId), newVer);
            return true;
        }

        public async Task<bool> Delete(string id)
        {
            var DeleteRecored = await _context.Users.DeleteOneAsync(Builders<UserModel>.Filter.Eq("UserId", id));
            return DeleteRecored.DeletedCount > 0;
        }

        public async Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression)
        {
            return await _context.Users
                    .Find(expression).ToListAsync();
        }

        public async Task<UserModel> GetById(string id)
        {
            var cursor = _context.Users.Find(x => x.UserId == id);
            var res = await cursor.FirstOrDefaultAsync();
            return res;
        }
    }
}


