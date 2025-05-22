using sd.Api.Helper;
using sd.Api.Infrastructure.Repositories;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression);
        Task<UserModel> GetById(string id);
        Task<bool> Create(UserModel entity);
        Task<bool> Update(UserModel entity);
        Task<bool> Delete(string id);

        Task<UserModel?> Create(string userEmail);
        Task<UserModel?> GetCurrentUser(ClaimsPrincipal user);
    }

    public class UserService : IUserService
    {
        private readonly CachingHelper _cacheHelper;
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, CachingHelper cacheHelper)
        {
            _cacheHelper = cacheHelper;
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
        public async Task<bool> Delete(string id)
        {
            return await _userRepository.Delete(id);
        }

        public async Task<UserModel?> Create(string email)
        {
            var result = await _userRepository.GetByCondation(u => u.Email == email);

            if (result?.First() != null)
                return result.First();
            else
            {
                UserModel userModel = new UserModel();
                userModel.UserId = Guid.NewGuid().ToString();
                userModel.CreatedAt = DateTime.Now;

                await _userRepository.Create(userModel);
                var res = await GetByCondation(u => u.Email == userModel.Email);
                return res.First();
            }
        }

        public async Task<bool> Update(UserModel newVer)
        {
            var oldVer = await _userRepository.GetById(newVer.UserId);
            if (oldVer is null || newVer == null)
            {
                return false;
            }

            if (oldVer.Email != newVer.Email)
            {
                var res = await _userRepository.GetByCondation(u => u.Email == newVer.Email);
                if (res?.First() != null)
                    return false; // {newVer.Email}  is already in use
                else newVer.IsEmailReg = false;
            }

            try
            {
                await _userRepository.Update(newVer);
            }
            catch
            {
                return false;
            }

            return true;
        }
        public async Task<UserModel?> GetCurrentUser(ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            var email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return null;

            string cacheKey = $"User-{email}";

            var userModel = _cacheHelper.GetValue<UserModel>(cacheKey);

            if (userModel == null)
                userModel = await Create(email);

            return _cacheHelper.SetValue<UserModel>(cacheKey, userModel);
        }

        public async Task<UserModel> GetById(string id)
        {
            return await _userRepository.GetById(id);
        }
    }
}