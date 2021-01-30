using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IOtherPageRepository
    {
        //Task<IEnumerable<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
        Task<IEnumerable<OtherPageModel>> FilterByLangs(string fromLang, string toLang);
        Task<OtherPageModel> GetOtherPageById(string id);
        Task<TransObj> RegisterOtherPage(OtherPageModel otherPage);
        Task<TransObj> UpdateOtherPage(string id, OtherPageModel newOtherPage);
        Task<bool> RemoveOtherPage(string id);
    }
}
