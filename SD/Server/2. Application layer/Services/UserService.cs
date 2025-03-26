using sd.Api.Infrastructure.Repositories;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IUserService: ICrudBase<UserModel>
    {
        Task<List<UserModel>> SearchUser(string searchText);
        Task<UserModel?> RegisterUserAsync(string userEmail);
        Task<TransObj> UpdateUser(string id, UserModel newVer);
        Task<UserModel?> GetCurrentUser(ClaimsPrincipal user);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression)
        {
            return await _userRepository.GetByCondation(expression);
        }
        public async Task<bool> Create(UserModel entity)
        {
            return await _userRepository.Create(entity);
        }
        public async Task<bool> Update(UserModel entity)
        {
            return await _userRepository.Update(entity);
        }
        public async Task<bool> Delete(string id)
        {
            return await _userRepository.Delete(id);
        }

        public async Task<List<UserModel>> SearchUser(string searchText)
        {
            var res= await _userRepository.GetByCondation(u => u.Name.ToLower().Contains(searchText.ToLower()));
            return res.ToList();
        }

        public async Task<UserModel?> RegisterUserAsync(string email)
        {
            var result = await _userRepository.GetByCondation(u => u.Email == email);

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
            var oldVer = await _userRepository.GetByCondation(u => u.UserId == id);
            if (oldVer.Count() == 0 || newVer == null)
            {
                return new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." };
            }

            if (oldVer.First().Email != newVer.Email)
            {
                var res = await _userRepository.GetByCondation(u => u.Email == newVer.Email);
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
    }
}