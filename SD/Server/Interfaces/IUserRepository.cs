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
        Task<UserModel> GetUserByEmail(string email);
        Task<List<UserModel>> SearchUser(string searchText);
        Task<List<UserModel>> GetUsers(List<string> userIds);

        Task Create(UserModel user);
        Task Update(string id, UserModel newUser);
        Task<bool> Delete(string id);
    }
}
