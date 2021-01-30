using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;
using sd.Api.Interfaces;

namespace sd.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;

        public UserRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _context.Users
                    .Find(user => true).ToListAsync();
        }
        public async Task<List<UserModel>> SearchUser(string CurrentUserId, string searchText)
        {
            return await _context.Users
                .Find(u => u.Name.ToLower().Contains(searchText.ToLower())).ToListAsync();
        }

        public async Task<List<UserModel>> GetUsers(List<string> userIds)
        {
            return await _context.Users
                  .Find(u => userIds.Contains(u.UserId)).ToListAsync();
        }

        public async Task<UserModel> GetUserById(string id)
        {
            try
            {
                return await _context.Users.Find<UserModel>(u => u.UserId == id).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CheckEmail(string email)
        {
            return await _context.Users.Find<UserModel>(u => u.Email == email).AnyAsync();
        }

        public async Task RegisterUserAsync(UserModel user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task UpdateUser(string id, UserModel newVer)
        {
            await _context.Users.FindOneAndReplaceAsync(Builders<UserModel>.Filter.Eq("UserId", id), newVer);
        }

        public async Task<bool> RemoveUser(string id)
        {
            MongoDB.Driver.DeleteResult DeleteRecored;
            DeleteRecored = await _context.Users.DeleteOneAsync(Builders<UserModel>.Filter.Eq("UserId", id));
            return DeleteRecored.IsAcknowledged;
        }
    }
}


