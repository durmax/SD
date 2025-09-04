using AutoMapper;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using sd.Api.Infrastructure;
using sd.Api.Application_layer.Interfaces.Repositories;

namespace sd.Api.Application.Services
{
    public interface IWordService
    {
        Task<IEnumerable<WordModel>> GetByCondation(Expression<Func<WordModel, bool>> expression);
        Task<WordModel> GetById(string id);
        Task<bool> Create(WordModel entity);
        Task<bool> Update(WordModel entity);
        Task<bool> Delete(string id);

        Task<int> Like(string userId, string wordId);
        Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage);
        Task<WordDto> GetWordDtoById(string id, string? currentUserId);
        Task<WordDto> GetWordByText(string userId, string text);
        Task<IEnumerable<string>> GetWordsContainText(string userId, string text);
        Task<string> AddWord(WordDto wordDto);
        Task<IEnumerable<CommentModel?>> GetWordComments(string wordId);
        Task<bool> SaveComment(string wordId, CommentModel newComment);
        Task<int> LikeComment(string userId, string wordId, string commentId);
        Task<bool> DeleteComment(string currUsr, string wordId, string commentId);
        Task <string> GetAI(string wordTitle);
    }

    public class WordService : IWordService
    {
        private readonly IWordRepository _wordRepo;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepo;
        private readonly IRelationshipRepository _relationshipRepo;
        private readonly GeminiService _geminiService;

        public WordService(IWordRepository wordRepo, IMapper mapper, IUserRepository userRepo, IRelationshipRepository relationshipRepo, GeminiService geminiService)
        {
            _wordRepo = wordRepo;
            _mapper = mapper;
            _userRepo = userRepo;
            _relationshipRepo = relationshipRepo;
            _geminiService = geminiService;
        }

        public async Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage)
        {
            List<WordDto> wordDtos = new();
            WordModel word;
            WordDto wordDto;
            int newCurrentPage = currentPage;
            Tuple<int, List<WordDto>> Res;

            long wordsCount = await _wordRepo.GetDocCount(userId, lang);

            while (wordDtos?.Count < pageSize)
            {
                if (wordsCount < newCurrentPage)
                {
                    break;
                }
                word = await _wordRepo.GetWord(userId, lang, newCurrentPage, 1);
                if (word != null)
                {
                    wordDto = _mapper.Map<WordDto>(word);
                    if (await IsWordSharedWithUser(wordDto, currentUserId))
                    {
                        if (word.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                        if (currentUserId != wordDto?.UserId)
                        {
                            var user = await _userRepo.GetById(wordDto.UserId);
                            wordDto.UserName = user?.Name;
                        }
                        wordDtos.Add(wordDto);
                    }
                }
                newCurrentPage++;
            }

            return wordDtos;
        }

        public async Task<WordDto> GetWordDtoById(string id, string? currentUserId)
        {
            var word = await _wordRepo.GetById(id);
            var wordDto = _mapper.Map<WordDto>(word);

            if (await IsWordSharedWithUser(wordDto, currentUserId))
            {
                if (word?.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                if (currentUserId != wordDto?.UserId)
                {
                    var user = await _userRepo.GetById(wordDto.UserId);
                    wordDto.UserName = user?.Name;
                }
                return wordDto;
            }
            else return null;
        }

        private async Task<bool> IsWordSharedWithUser(WordDto wordDto, string? userId)
        {
            if (userId == wordDto?.UserId)
            {
                return true;
            }

            var rs = await _relationshipRepo.GetByCondation(x =>
                ( x.Reletion == Relation.Friend) &&
                ((x.UserId1 == userId && x.UserId2 == wordDto.UserId) ||
                 (x.UserId1 == wordDto.UserId && x.UserId2 == userId))
                );

            if (!string.IsNullOrEmpty(rs.FirstOrDefault()?.RelationshipId))
            {
                return wordDto.ShareWith == ShareWith.Friends || wordDto.ShareWith == ShareWith.Public;
            }
            else
            {
                return wordDto?.ShareWith == ShareWith.Public;
            }
        }

        public async Task<WordDto> GetWordByText(string userId, string text)
        {
            var words = await _wordRepo.GetByCondation(u => string.Equals(u.Title, text, StringComparison.OrdinalIgnoreCase) && u.UserId == userId);
            var word = words.FirstOrDefault();

            if (word != null)
            {
                word.Score++;
                await _wordRepo.Update(word);
            }
            return _mapper.Map<WordDto>(word);
        }

        public async Task<IEnumerable<string>> GetWordsContainText(string userId, string text)
        {
            List<string> res = new();
            var words = await _wordRepo.GetByCondation(u => u.Title.ToUpperInvariant().StartsWith(text.ToUpperInvariant()) && u.UserId == userId);

            foreach (var w in words)
            {
                res.Add($"{w.Title}:{w.WordId}");
            }
            return res;
        }

        public async Task<string> AddWord(WordDto wordDto)
        {
            var word = _mapper.Map<WordModel>(wordDto);
            word.WordId = Guid.NewGuid().ToString();
            word.CreatedAt = DateTime.Now;
            if (await _wordRepo.Create(word)) return word.WordId;
            else return null;
        }

        public async Task<bool> Update(WordModel word)
        {
            var oldWord = await _wordRepo.GetById(word.WordId);

            if (oldWord == null) return false;

            word.Comments = oldWord.Comments;
            word.Likes = oldWord.Likes;
            word.CreatedAt = DateTime.Now;
            return await _wordRepo.Update(word);
        }

        public async Task<IEnumerable<CommentModel?>> GetWordComments(string wordId)
        {
            var word = await _wordRepo.GetById(wordId);
            return word?.Comments;
        }

        public async Task<bool> SaveComment(string wordId, CommentModel newComment)
        {
            try
            {
                var word = await _wordRepo.GetById(wordId);
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
                    await _wordRepo.Update(word);
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> LikeComment(string userId, string wordId, string commentId)
        {
            var word = await _wordRepo.GetById(wordId);
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

        public async Task<bool> DeleteComment(string currUsr, string wordId, string commentId)
        {
            var word = await _wordRepo.GetById(wordId);

            CommentModel comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return false;
            if (comment.UserId != currUsr) return false;

            if (word.Comments.Contains(comment))
            {
                word.Comments.Remove(comment);
                await _wordRepo.Update(word);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<WordModel>> GetByCondation(Expression<Func<WordModel, bool>> expression)
        {
            return await _wordRepo.GetByCondation(expression);
        }

        public async Task<bool> Create(WordModel entity)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Delete(string id)
        {
            return await _wordRepo.Delete(id);
        }

        public async Task<int> Like(string userId, string wordId)
        {
            var wordModel = await _wordRepo.GetById(wordId);
            if (wordModel != null)
            {
                if (wordModel.Likes == null) wordModel.Likes = new List<string>();
                if (!wordModel.Likes.Contains(userId))
                {
                    wordModel.Likes.Add(userId);
                }
                else
                {
                    wordModel.Likes.Remove(userId);
                }
                await _wordRepo.Update(wordModel);
                return wordModel.Likes.Count();
            }
            else return -1;
        }

        public async Task<string> GetAI(string wordTitle)
        {
            var result = await _geminiService.ProcessStringAsync($"Schreibe mir Beispiele auf Niveau B1, die mir die Bedeutungen von '{wordTitle}' zu versehen hilft.");
            return result;
        }

        public async Task<WordModel> GetById(string id)
        {
            return await _wordRepo.GetById(id);
        }
    }
}
