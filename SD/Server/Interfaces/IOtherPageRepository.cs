using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IOtherPageRepository
    {
        //Task<IEnumerable<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
        Task<List<OtherPageModel>> FilterByLangs(string fromLang, string toLang);
        Task<OtherPageModel> GetOtherPageById(string id);
        Task<TransObj> Create(OtherPageModel otherPage);
        Task<TransObj> Update(string id, OtherPageModel newOtherPage);
        Task<bool> Delete(string id);
    }
}
