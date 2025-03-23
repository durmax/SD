using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IUserRepository: ICrudBase<UserModel>
    {
        Task<List<UserModel>> SearchUser(string searchText);
        Task<UserModel?> RegisterUserAsync(string userEmail);
        Task<TransObj> UpdateUser(string id, UserModel newVer);
        Task<UserModel?> GetCurrentUser(ClaimsPrincipal user);
    }
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;

        public UserRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<List<UserModel>> SearchUser(string searchText)
        {
            return await _context.Users
                .Find(u => u.Name.ToLower().Contains(searchText.ToLower())).ToListAsync();
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
            DeleteResult DeleteRecored;
            DeleteRecored = await _context.Users.DeleteOneAsync(Builders<UserModel>.Filter.Eq("UserId", id));
            return DeleteRecored.DeletedCount > 0;
        }

        public async Task<UserModel?> RegisterUserAsync(string email)
        {
            var result = await GetByCondation(u => u.Email == email);

            if (result?.First() != null)
                return result.First();
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserId = Guid.NewGuid().ToString();
                userModel.CreatedAt = DateTime.Now;

                await Create(userModel);
                var res = await GetByCondation(u => u.Email == userModel.Email);
                return res.First();
            }
        }

        public async Task<TransObj> UpdateUser(string id, UserModel newVer)
        {
            var oldVer = await GetByCondation(u => u.UserId == id);
            if (oldVer.Count() == 0 || newVer == null)
            {
                return new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." };
            }

            if (oldVer.First().Email != newVer.Email)
            {
                var res = await GetByCondation(u => u.Email == newVer.Email);
                if (res?.First() != null)
                    return new TransObj { BoolVar = false, SetringVar = $"Sorry, {newVer.Email}  is already in use." };
                else newVer.IsEmailReg = false;
            }

            try
            {
                await Update(newVer);
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

        public async Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression)
        {
            return await _context.Users
                    .Find(expression).ToListAsync();
        }
    }
}


