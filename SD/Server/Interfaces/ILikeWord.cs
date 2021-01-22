using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
   public interface ILikeWord
    {
        Task<int> Like(string userId, string wordId);
    }
}
