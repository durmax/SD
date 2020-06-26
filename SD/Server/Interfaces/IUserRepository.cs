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
        Task<IEnumerable<UserModel>> SearchUser(string text);
        Task<bool> CheckEmail(string email);
        Task<TransObj> RegisterUserAsync(UserModel user);
        Task<TransObj> UpdateUser(string id, UserModel newUser);
        Task<bool> RemoveUser(string id);

        Task AddFriendRequest(string userIdAndName, string friendId);
        Task AddFriend(string UserId, string friendId);
        Task RemoveFriend(string UserId, string friendId);
        Task RemoveFriendRequest(string UserId, string friendId);
    }
}
