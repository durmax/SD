using System.Linq.Expressions;
using MongoDB.Driver;
using sd.Infrastructure.Models;
using sd.Application.Interfaces.Repositories;
using sd.Shared;

namespace sd.Infrastructure.Repositories
{
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

        public async Task<IEnumerable<UserModel>> GetByCondition(Expression<Func<UserModel, bool>> expression)
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


