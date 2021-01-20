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

        public UserRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _context.Users
                    .Find(user => true).ToListAsync();
        }
        public async Task<Dictionary<string, Tuple<string, string>>> SearchUser(string CurrentUserId, string searchText)
        {
            var foundUsers = await _context.Users
                .Find(u => u.Name.ToLower().Contains(searchText.ToLower())).ToListAsync();

            return await GetRelationships(CurrentUserId, foundUsers);
        }

        public async Task<Dictionary<string, Tuple<string, string>>> GetUsersWithRelationship(string CurrentUserId, List<string> userIds)
        {
           var foundUsers = await _context.Users
                 .Find(u => userIds.Contains(u.UserId)).ToListAsync();
            
            return await GetRelationships(CurrentUserId, foundUsers);
        }

        public async Task<Dictionary<string, Tuple<string, string>>> GetRelationships(string CurrentUserId, List<UserModel> users)
        {
            string relation = null;
            UserModel crrUser = null;
            Dictionary<string, Tuple<string, string>> res = new Dictionary<string, Tuple<string, string>>();
 
            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                crrUser = await GetUserById(CurrentUserId);
                users.RemoveAll(u => u.UserId == CurrentUserId); //remove Sercher from list
            }

            foreach (var user in users)
            {
                if (crrUser != null)
                {
                    if (user.Friends == null) user.Friends = new List<string>();
                    if (user.FriendRequests == null) user.FriendRequests = new List<string>();

                    if (user.Friends.Contains(CurrentUserId))
                    {
                        relation = "Friends";
                    }

                    else if (user.FriendRequests.Contains(CurrentUserId))
                    {
                        relation = "CrrRequest";
                    }

                    else if (crrUser.FriendRequests.Contains(user.UserId))
                    {
                        relation = "UserRequest";
                    }
                }

                var userT = Tuple.Create(user.Name, relation);
                res.Add(user.UserId, userT);

                relation = null;
            }
            return res;
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

        public async Task AddFriendRequest(string userId, string friendId)
        {
            UserModel user = await GetUserById(friendId);

            if (user.Friends == null) user.Friends = new List<string>();
            if (user.FriendRequests == null) user.FriendRequests = new List<string>();

            if (!user.Friends.Contains(userId))
            {
                if (!user.FriendRequests.Contains(userId))
                {
                    user.FriendRequests.Add(userId);
                    await UpdateUser(friendId, user);
                }
            }
        }
        public async Task RemoveFriendRequest(string UserId, string friendId)
        {
            UserModel user = await GetUserById(UserId);
            if (user.FriendRequests != null)
            {
                user.FriendRequests.Remove(friendId);
                await UpdateUser(UserId, user);
            }
        }
        public async Task AddFriend(string UserId, string friendId)
        {
            UserModel userModel = await GetUserById(UserId);
            if (userModel.Friends == null) userModel.Friends = new List<string>();

            if (!userModel.Friends.Contains(friendId))
            {
                userModel.Friends.Add(friendId);
                await UpdateUser(UserId, userModel);
            }

            UserModel friendModel = await GetUserById(friendId);
            if (friendModel.Friends == null) friendModel.Friends = new List<string>();

            if (!friendModel.Friends.Contains(UserId))
            {
                friendModel.Friends.Add(UserId);
                await UpdateUser(friendId, friendModel);
            }

        }
        public async Task RemoveFriend(string UserId, string friendId)
        {
            UserModel userModel = await GetUserById(UserId);
            if (userModel != null)
            {
                if (userModel.Friends != null)
                {
                    userModel.Friends.RemoveAll(u => u.Contains(friendId));
                    await UpdateUser(UserId, userModel);
                }
            }

            UserModel friendModel = await GetUserById(friendId);
            if (friendModel != null)
            {
                if (friendModel.Friends != null)
                {
                    friendModel.Friends.RemoveAll(u => u.Contains(UserId));
                    await UpdateUser(friendId, friendModel);
                }
            }
        }

        public async Task<Dictionary<string, string>> GetAllFriends(string userId)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            var users = await _context.Users
                .Find(u => u.Friends.Contains(userId)).ToListAsync();
            foreach (var user in users)
            {
                res.Add(user.UserId, user.Name);
            }

            return res;
        }
    }
}


