using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<CommentModelWithOwnerName>> GetAllComments(string WordId);
        Task<CommentModel> GetComment(string commentId);
        Task<bool> AddComment(CommentModel comment);
        Task<bool> UpdateComment(CommentModel newComment);
        Task<bool> RemoveComment(string commentId);
        Task<int> Like(string userId, string commentId);
    }
}
