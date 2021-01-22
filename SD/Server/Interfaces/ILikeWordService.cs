using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
   public interface ILikeWordService
    {
        Task<int> Like(string userId, string wordId);
    }
}
