using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sd.Api.Interfaces;
using SD.Shared;

namespace sd.Api.Services
{
    public class OtherPageService
    {
        private readonly IOtherPageRepository _otherPageRepository;

        public OtherPageService(IOtherPageRepository otherPageRepository)
        {
            _otherPageRepository = otherPageRepository;
        }

        public async Task<IEnumerable<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {
            List<OtherPageResModel> res = new List<OtherPageResModel>();

            var otherPages = await _otherPageRepository.FilterByLangs(fromLang, toLang);

            //otherPages.Sort((x, y) => x.Order.CompareTo(y.Order));

            if (otherPages != null)
            {
                foreach (var otherPage in otherPages)
                {
                    if (!string.IsNullOrEmpty(otherPage.Pattern))
                    {
                        OtherPageResModel otherPageResModel = new OtherPageResModel();
                        otherPageResModel.Pattern = otherPage.Pattern;
                        otherPageResModel.Host = otherPage.Host;
                        otherPageResModel.Type = otherPage.PageType;
                        otherPageResModel.Eval = otherPage.Eval;
                        res.Add(otherPageResModel);
                    }
                }
                // return res;
            }
            return res.OrderBy(o => o.Eval);
        }

        public async Task<OtherPageModel> GetOtherPageById(string id)
        {
            return await _otherPageRepository.GetOtherPageById(id);
        }

        public async Task<TransObj> RegisterOtherPage(OtherPageModel otherPage)
        {
            return await _otherPageRepository.RegisterOtherPage(otherPage);
        }

        public async Task<TransObj> UpdateOtherPage(string id, OtherPageModel newOtherPage)
        {
            return await _otherPageRepository.UpdateOtherPage(id, newOtherPage);
        }

        public async Task<bool> RemoveOtherPage(string id)
        {
            return await _otherPageRepository.RemoveOtherPage(id);
        }
    }
}
