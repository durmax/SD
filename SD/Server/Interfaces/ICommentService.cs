using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentModel>> GetWordComments(string wordId);
        Task<bool> SaveComment(string wordId, CommentModel newComment);
        Task<int> Like(string userId, string wordId, string commentId);
        Task<bool> Remove(string currUsr, string wordId, string commentId);
    }
}
