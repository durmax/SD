using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using sd.Api.Helpers;
using sd.Api.Models;
using SD.Shared;
using sd.Api.Interfaces;

namespace sd.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongodbContext _context = null;
        private readonly GetRelationshipsService _GetRelationshipsService;

        public UserRepository(MongodbContext mongodbContext, GetRelationshipsService GetRelationshipsService)
        {
            _context = mongodbContext;
            _GetRelationshipsService = GetRelationshipsService;
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
            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                UserModel crrUser = await GetUserById(CurrentUserId);
                return _GetRelationshipsService.GetRelationships(crrUser, users);
            }
            else return null;
        }

        public async Task<UserModel> GetUserById(string id)
        {
            try
            {
                return await _context.Users.Find<UserModel>(u => u.UserId == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Dictionary<string, string>> GetFriendRequestsById(string id)
        {
            Dictionary<string, string> FriendRequestsDictionary = new Dictionary<string, string>();
            try
            {
                UserModel user = await _context.Users.Find<UserModel>(u => u.UserId == id).FirstOrDefaultAsync();

                foreach (var idFR in user.FriendRequests)
                {
                    UserModel user1 = await GetUserById(idFR);
                    FriendRequestsDictionary.Add(user1.UserId, user1.Name);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return FriendRequestsDictionary;
        }

        public async Task<UserModel> GetUserByPost(TransObj status)
        {
            try
            {
                return await _context.Users.Find<UserModel>(u => u.UserId == status.SetringVar).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                return null;
            }
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
                user.CreatedAt = DateTime.Now;
                await _context.Users.InsertOneAsync(user);
                return new TransObj { BoolVar = true, SetringVar = "User Details Inserted Successfully" };
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
                if (await CheckEmail(newVer.Email))
                    return new TransObj { BoolVar = false, SetringVar = $"Sorry, {newVer.Email}  is already in use." };
                else newVer.IsEmailReg = false;
            }

            try
            {
                await _context.Users.FindOneAndReplaceAsync(Builders<UserModel>.Filter.Eq("UserId", id), newVer);
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
            DeleteRecored = await _context.Users.DeleteOneAsync(Builders<UserModel>.Filter.Eq("UserId", id));
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
            UserModel userModel;
            userModel = await GetUserById(UserId);
            await AddToFriendsAsync(userModel, friendId);

            userModel = await GetUserById(friendId);
            await AddToFriendsAsync(userModel, UserId);
        }

        private async Task AddToFriendsAsync(UserModel userModel, string friendId)
        {
            if (!userModel.Friends.Contains(friendId))
            {
                if (userModel.Friends == null) userModel.Friends = new List<string>();
                userModel.Friends.Add(friendId);
                await UpdateUser(userModel.UserId, userModel);
            }
        }

        public async Task RemoveFriend(string UserId, string friendId)
        {
            await RemoveFromFriendsAsync(UserId, friendId);
            await RemoveFromFriendsAsync(friendId, UserId);
        }

        private async Task RemoveFromFriendsAsync(string UserId, string friendId)
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


