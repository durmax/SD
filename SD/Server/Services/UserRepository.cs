using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;

namespace sd.Api.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;

        public UserRepository(IMongodbSettings settings)
        {
            _context = new MongodbContext(settings);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _context.Users
                    .Find(user => true).ToListAsync();
        }
        public async Task<IEnumerable<UserModel>> SearchUser(string text)
        {
            var users = await _context.Users
                .Find(u => u.Name.Contains(text) || u.Email.Contains(text)).ToListAsync();
            return users;
        }
        public async Task<UserModel> GetUserById(string id)
        {
            bool userIsExist = await _context.Users.Find<UserModel>(u => u.UserId == id).AnyAsync();
            if (userIsExist)
            {
                var user = await _context.Users.Find<UserModel>(u => u.UserId == id).FirstOrDefaultAsync();

                // Decrypt the Password
                //user.Password = _protector.Unprotect(user.Password);
                return user;
            }
            else return null;
        }

        public async Task<UserModel> GetUserByPost(TransObj status)
        {
            bool userIsExist = await _context.Users.Find<UserModel>(u => u.UserId == status.SetringVar).AnyAsync();
            if (userIsExist)
            {
                var user = await _context.Users.Find<UserModel>(u => u.UserId == status.SetringVar).FirstOrDefaultAsync();

                // Decrypt the Password
                //user.Password = _protector.Unprotect(user.Password);
                return user;
            }
            else return null;
        }

        public async Task<bool> CheckEmail(string email)
        {
            return await _context.Users.Find<UserModel>(u => u.Email == email).AnyAsync();
        }

        public async Task<TransObj> RegisterUserAsync(UserModel user)
        {
            // Add custom model validation error
            bool IsEmailExist = await CheckEmail(user.Email);

            if (!IsEmailExist)
            {
                // Encrypt the Password value and store in Password property
                //user.AccessToken = _protector.Protect(user.Email + user.Password);
                //user.Password = _protector.Protect(user.Password);

                user.CreatedAt = DateTime.Now;

                await _context.Users.InsertOneAsync(user);
                return new TransObj
                { BoolVar = true, SetringVar = "User Details Inserted Successfully" };
            }
            else
            {
                return new TransObj { BoolVar = false, SetringVar = $"Sorry, {user.Email}  is already in use." };
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
                bool IsEmailExist = await CheckEmail(newVer.Email);
                if (IsEmailExist)
                {
                    return new TransObj { BoolVar = false, SetringVar = $"Sorry, {newVer.Email}  is already in use." };
                }
                else
                {
                    newVer.IsEmailReg = false;
                }
            }

            try
            {
                //newVer.AccessToken = _protector.Protect(newVer.Email + newVer.Password);
                //newVer.Password = _protector.Protect(newVer.Password);

                await _context.Users.FindOneAndReplaceAsync(
      Builders<UserModel>.Filter.Eq("UserId", id), newVer);

            }
            catch
            {
                return new TransObj { BoolVar = false, SetringVar = "Sorry, update data error" };
            }

            return new TransObj { BoolVar = true, SetringVar = "Your data updated successfully" };

        }

        public async Task<bool> RemoveUser(string id)
        {
            MongoDB.Driver.DeleteResult DeleteRecored;
            DeleteRecored = await _context.Users.DeleteOneAsync(
              Builders<UserModel>.Filter.Eq("UserId", id));
            return DeleteRecored.IsAcknowledged;
        }

        public async Task AddFriendRequest(string userIdAndName,  string friendId)
        {
            UserModel user = await GetUserById(friendId);
            user.FriendRequests.Add(userIdAndName);
            await UpdateUser(friendId, user);
        }  
        public async Task RemoveFriendRequest(string UserId, string friendId)
        {
            UserModel user = await GetUserById(UserId);
            user.FriendRequests.Remove(friendId);
            await UpdateUser(UserId, user);
        }
        public async Task AddFriend(string UserId, string friendId)
        {
            UserModel user = await GetUserById(UserId);
            user.Friends.Add(friendId);
            await UpdateUser(UserId, user);
        }
        public async Task RemoveFriend(string UserId, string friendId)
        {
            UserModel user = await GetUserById(UserId);
            user.Friends.Remove(friendId);
            await UpdateUser(UserId, user);
        }
    }
}


