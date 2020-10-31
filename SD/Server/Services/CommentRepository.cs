using MongoDB.Driver;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class CommentRepository : ICommentRepository
    {

        private readonly MongodbContext _context;
        private readonly IWordRepository _wordService;

        public CommentRepository(IMongodbSettings settings, IWordRepository wordService)
        {
            _context = new MongodbContext(settings);
            _wordService = wordService;
        }
        public async Task<CommentModel> GetComment(string commentId)
        {
          return await _context.Comments.Find(c => c.CommentId == commentId).FirstAsync();
        }
        public async Task<bool> AddComment(CommentModel comment)
        {
            try
            {
                await _context.Comments.InsertOneAsync(comment);
                return true;
            }
            catch (Exception Ex)
            {
                if (Ex.Message.Contains("duplicate key error"))
                {
                    return await UpdateComment(comment);
                }
                else return false;
            }
        }

        public async Task<bool> RemoveComment(string commentId)
        {
            try
            {
                await _context.Comments.DeleteOneAsync(c => c.CommentId == commentId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateComment(CommentModel newComment)
        {
            try
            {
                await _context.Comments.ReplaceOneAsync(c => c.CommentId == newComment.CommentId, newComment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> Like(string userId, string commentId)
        {
            CommentModel comment = await GetComment(commentId);

            if (comment == null) comment.Likes = new List<string>();
            if (!comment.Likes.Contains(userId))
            {
                comment.Likes.Add(userId);
            }
            else
            {
                comment.Likes.Remove(userId);
            }
            await UpdateComment(comment);
            return comment.Likes.Count();
        }

        public async Task<IEnumerable<CommentModelWithOwnerName>> GetAllComments(string WordId)
        {
           var word= await _wordService.GetWordById(WordId);
            List<CommentModel> comments = new List<CommentModel>();
            List<CommentModelWithOwnerName> commentsWithName = new List<CommentModelWithOwnerName>();
            foreach (var id in word.Comments)
            {
                comments.Add(await _context.Comments.Find(c => c.CommentId == id).FirstAsync());
            }

            if (comments!=null)
            {
                foreach (var comment in comments)
                {
                    CommentModelWithOwnerName modelWithNames = new CommentModelWithOwnerName();
                    modelWithNames = (CommentModelWithOwnerName)comment;

                   var user = await _context.Users.Find<UserModel>(u => u.UserId == comment.UserId).FirstOrDefaultAsync();
                    modelWithNames.CommentOwnerName = user.Name;
                    commentsWithName.Add(modelWithNames);
                }
            }
            return commentsWithName;
        }
    }
}
