using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<UserModel> GetUserById(string id);
        Task<UserInfo> GetUserInfoById(string id);

        Task<bool> CheckEmail(string email);
        Task<TransObj> RegisterUserAsync(UserModel user);
        Task<TransObj> UpdateUser(string id, UserModel newUser);
        Task<bool> RemoveUser(string id);

        Task AddFriendRequest(string userId, string friendId);
        Task AddFriend(string UserId, string friendId);
        Task RemoveFriend(string UserId, string friendId);
        Task RemoveFriendRequest(string UserId, string friendId);

        Task<Dictionary<string, string>> GetAllFriends(string userId);

        Task<Dictionary<string, Tuple<string, string>>> SearchUser(string searcherId, string searchText);
        Task<Dictionary<string, Tuple<string, string>>> GetUsersWithRelationship(string searcherId, List<string> userIds);
    }
}
