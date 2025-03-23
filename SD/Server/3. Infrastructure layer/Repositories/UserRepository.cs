using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel> GetUserById(string id);
        Task<UserModel> GetUserByEmail(string email);
        Task<List<UserModel>> SearchUser(string searchText);
        Task<List<UserModel>> GetUsers(List<string> userIds);

        Task Create(UserModel user);
        Task Update(string id, UserModel newUser);
        Task<bool> Delete(string id);

        Task<UserModel?> RegisterUserAsync(string userEmail);
        Task<TransObj> UpdateUser(string id, UserModel newVer);

        Task<UserModel?> GetCurrentUser(ClaimsPrincipal user);
    }
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;
        private readonly IUserRepository _userRepository;

        public UserRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _context.Users
                    .Find(user => true).ToListAsync();
        }
        public async Task<List<UserModel>> SearchUser(string searchText)
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

        public async Task<UserModel> GetUserByEmail(string email)
        {
            return await _context.Users.Find<UserModel>(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task Create(UserModel user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task Update(string id, UserModel newVer)
        {
            await _context.Users.FindOneAndReplaceAsync(Builders<UserModel>.Filter.Eq("UserId", id), newVer);
        }

        public async Task<bool> Delete(string id)
        {
            MongoDB.Driver.DeleteResult DeleteRecored;
            DeleteRecored = await _context.Users.DeleteOneAsync(Builders<UserModel>.Filter.Eq("UserId", id));
            return DeleteRecored.DeletedCount > 0;
        }

        public async Task<UserModel?> RegisterUserAsync(string userEmail)
        {
            var existingUser = await GetUserByEmail(userEmail);

            if (existingUser != null)
                return existingUser;
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserId = Guid.NewGuid().ToString();
                userModel.CreatedAt = DateTime.Now;

                await _userRepository.Create(userModel);

                return await GetUserByEmail(userModel.Email);
            }
        }

        public async Task<TransObj> UpdateUser(string id, UserModel newVer)
        {
            UserModel oldVer = await GetUserById(id);
            if (oldVer == null || newVer == null)
            {
                return new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." };
            }

            if (oldVer.Email != newVer.Email)
            {
                if (await GetUserByEmail(newVer.Email) != null)
                    return new TransObj { BoolVar = false, SetringVar = $"Sorry, {newVer.Email}  is already in use." };
                else newVer.IsEmailReg = false;
            }

            try
            {
                await _userRepository.Update(id, newVer);
            }
            catch
            {
                return new TransObj { BoolVar = false, SetringVar = "Sorry, update data error" };
            }

            return new TransObj { BoolVar = true, SetringVar = "Your data updated successfully" };
        }
        public async Task<UserModel?> GetCurrentUser(ClaimsPrincipal user)
        {
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;

                return await RegisterUserAsync(email);
            }
            else return null;
        }
    }
}


