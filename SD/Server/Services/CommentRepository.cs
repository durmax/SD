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
    public class CommentRepository : ICommentRepository
    {
        private readonly IWordRepository _wordService;

        public CommentRepository(IWordRepository wordService)
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
                    await _wordService.UpdateWord(wordId, word);
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
    }
}
