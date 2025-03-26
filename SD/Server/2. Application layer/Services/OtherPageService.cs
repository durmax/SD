using sd.Api.Infrastructure.Repositories;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IOtherPageService : ICrudBase<OtherPageModel>
    {
        Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
        Task<List<OtherPageResModel>> GetOtherPageResModels(string fromLang, string toLang);
        Task<bool> AddOtherPage(OtherPageModel otherPage);
        Task<bool> ModifyOtherPage(OtherPageModel otherPage);
        Task<bool> RemoveOtherPage(string id);
    }
    public class OtherPageService : IOtherPageService
    {
        private readonly IOtherPageRepository _otherPageRepository;

        public OtherPageService(IOtherPageRepository otherPageRepository)
        {
            _otherPageRepository = otherPageRepository;
        }

        public async Task<List<OtherPageResModel>> GetOtherPageResModels(string fromLang, string toLang)
        {
            return await _otherPageRepository.GetOPResModels(fromLang, toLang);
        }

        public async Task<bool> AddOtherPage(OtherPageModel otherPage)
        {
            return await _otherPageRepository.Create(otherPage);
        }

        public async Task<bool> ModifyOtherPage(OtherPageModel otherPage)
        {
            return await _otherPageRepository.Update(otherPage);
        }

        public async Task<bool> RemoveOtherPage(string id)
        {
            return await _otherPageRepository.Delete(id);
        }

        public async Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression)
        {
            return await _otherPageRepository.GetByCondation(expression);
        }

        public Task<bool> Create(OtherPageModel entity)
        {
            return _otherPageRepository.Create(entity);
        }

        public Task<bool> Update(OtherPageModel entity)
        {
            return _otherPageRepository.Update(entity);
        }

        public Task<bool> Delete(string id)
        {
            return _otherPageRepository.Delete(id);
        }

        public Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {
            return _otherPageRepository.GetOPResModels(fromLang, toLang);
        }
    }
}
