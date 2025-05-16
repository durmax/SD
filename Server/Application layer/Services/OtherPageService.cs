using sd.Api.Infrastructure.Repositories;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IOtherPageService : ICrudBase<OtherPageModel>
    {
        Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
    }

    public class OtherPageService : IOtherPageService
    {
        private readonly IOtherPageRepository _otherPageRepository;

        public OtherPageService(IOtherPageRepository otherPageRepository)
        {
            _otherPageRepository = otherPageRepository;
        }

        public async Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {
            List<OtherPageResModel> res = new List<OtherPageResModel>();


            var otherPages = await _otherPageRepository.GetByCondation(o =>
                  (o.PrimLangs == "All" && o.SecLangs == "All")
               || (o.PrimLangs.Contains(fromLang) && (o.SecLangs.Contains(toLang) || o.SecLangs == "All"))
               || (o.PrimLangs.Contains(toLang) && o.SecLangs.Contains(fromLang))
                   );

            if (otherPages != null)
            {
                int i = 0;
                foreach (var otherPage in otherPages)
                {
                    if (!string.IsNullOrEmpty(otherPage.Pattern))
                    {
                        OtherPageResModel otherPageResModel = new OtherPageResModel()
                        {
                            Pattern = otherPage.Pattern,
                            Host = otherPage.Host,
                            Type = otherPage.PageType,
                            Eval = otherPage.Eval == 0 ? i++ : otherPage.Eval
                        };
                        res.Add(otherPageResModel);
                    }
                }
                // return res;
            }
            return res.OrderBy(o => o.Eval).ToList();
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
    }
}
