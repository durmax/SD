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
        Task<bool> CheckEmail(string email);
        Task RegisterUserAsync(UserModel user);
        Task UpdateUser(string id, UserModel newUser);
        Task<bool> RemoveUser(string id);
        Task<List<UserModel>> SearchUser(string searcherId, string searchText);
        Task<List<UserModel>> GetUsers(List<string> userIds);
    }
}
