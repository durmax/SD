using MongoDB.Driver;
using MongoDB.Driver.Builders;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly IWordRepository _wordService;

        public CommentService(IWordRepository wordService)
        {
            _wordService = wordService;
        }

        public async Task<bool> SaveComment(string wordId, CommentModel newComment)
        {
            try
            {
                WordModel word = await _wordService.GetWordById(wordId);
                if (word != null)
                {
                    if (word.Comments != null)
                    {
                        CommentModel comment = word.Comments.SingleOrDefault(x => x.CommentId == newComment.CommentId);
                        if (comment != null)
                        {
                            word.Comments.Remove(comment);
                            newComment.UpdatedAt = DateTime.Now;
                        }
                    }
                    else
                    {
                        word.Comments = new List<CommentModel>();
                    }

                    word.Comments.Add(newComment);
                    await _wordService.Update(wordId, word);
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> Like(string userId, string wordId, string commentId)
        {
            WordModel word = await _wordService.GetWordById(wordId);
            CommentModel comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return 0;
            if (comment == null) comment.Likes = new List<string>();

            if (!comment.Likes.Contains(userId))
            {
                comment.Likes.Add(userId);
            }
            else
            {
                comment.Likes.Remove(userId);
            }
            await SaveComment(wordId, comment);
            return comment.Likes.Count();
        }

        public async Task<bool> Delete(string currUsr, string wordId, string commentId)
        {
            WordModel word = await _wordService.GetWordById(wordId);

            CommentModel comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return false;
            if (comment.UserId != currUsr) return false;

            if (word.Comments.Contains(comment))
            {
                word.Comments.Remove(comment);
                await _wordService.Update(wordId, word);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<CommentModel?>> GetWordComments(string wordId)
        {
             WordModel word = await _wordService.GetWordById(wordId);
            return word?.Comments;
        }
    }
}
