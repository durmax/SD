using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface ICommentRepository
    {
        Task<bool> SaveComment(string wordId, CommentModel newComment);
        Task<bool> RemoveComment(string wordId, string commentId);
        Task<int> Like(string userId, string wordId, string commentId);
    }
}
