using sd.Api.Interfaces;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserModel?> GetCurrentUser(ClaimsPrincipal user)
        {
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
                return await GetUserByEmail(email);
            }
            else return null;
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<List<UserModel>> SearchUser(string searchText)
        {
            return await _userRepository.SearchUser(searchText);
        }

        public async Task<List<UserModel>> GetUsers(List<string> userIds)
        {
            return await _userRepository.GetUsers(userIds);
        }

        public async Task<UserModel> GetUserById(string id)
        {
            return await _userRepository.GetUserById(id);
        }

        public async Task<UserModel?> GetUserByEmail(string? email)
        {
            return email != null ? await _userRepository.GetUserByEmail(email) : null;
        }

        public async Task<UserModel?> RegisterUserAsync(UserModel user)
        {
            var existingUser = await GetUserByEmail(user.Email);

            if (existingUser == null)
            {
                if (string.IsNullOrEmpty(user.UserId))
                {
                    user.UserId = Guid.NewGuid().ToString();
                }
                user.CreatedAt = DateTime.Now;

                await _userRepository.Create(user);
            }
            else
            {
                user = existingUser;
            }
            return user;
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

        public async Task<bool> RemoveUser(string id)
        {
            return await _userRepository.Delete(id);
        }
    }
}
