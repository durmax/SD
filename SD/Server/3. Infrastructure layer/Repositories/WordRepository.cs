using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Helpers;
using sd.Api.Models;
using sd.Api.Repositories;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IWordRepository
    {
        Task<long> GetDocCount(string userId, string lang);
        Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit);
        Task<WordModel> GetWordById(string id);
        Task<IEnumerable<WordModel>> GetWordsContainTextWWW(string userId, string text);

        Task<bool> Create(WordModel word);
        Task<bool> Update(string id, WordModel newWord);
        Task<bool> Delete(string id);

        Task<int> Like(string userId, string wordId);

        Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage);
        Task<bool> IsWordSharedWithUser(WordDto wordDto, string? userId);
        Task<WordDto> GetWordDtoById(string id, string? currentUserId);
        Task<WordDto> GetWordByText(string userId, string text);
        Task<IEnumerable<string>> GetWordsContainText(string userId, string text);
        Task<string> AddWord(WordDto wordDto);
        Task<bool> UpdateWord(WordDto updatedWordDto);

        Task<IEnumerable<CommentModel?>> GetWordComments(string wordId);
        Task<bool> SaveComment(string wordId, CommentModel newComment);
        Task<int> LikeComment(string userId, string wordId, string commentId);
        Task<bool> DeleteComment(string currUsr, string wordId, string commentId);

    }
    public class WordRepository : IWordRepository
    {
        private readonly MongodbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WordRepository> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IRelationshipRepository _relationshipRepo;

        public WordRepository(MongodbContext mongodbContext, IMapper mapper, ILogger<WordRepository> logger, IUserRepository userRepos, IRelationshipRepository relationshipRepo)
        {
            _context = mongodbContext;
            this._mapper = mapper;
            _logger = logger;
            this._userRepo = userRepos;
            this._relationshipRepo = relationshipRepo;
        }

        public async Task<long> GetDocCount(string userId, string lang)
        {
            var filter = WordHelper.GetFilter(null, userId, lang);
            return await _context.Words.CountDocumentsAsync(filter);
        }

        public async Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit)
        {
            var filter = WordHelper.GetFilter(null, userId, lang);
            var sort = Builders<WordModel>.Sort.Descending("Score").Descending("CreatedAt");
            try
            {
                return await _context.Words.Find(filter).Sort(sort).Skip(currentPage - 1).Limit(limit).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<WordModel> GetWordById(string id)
        {
            return await _context.Words.Find(w => w.WordId == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WordModel>> GetWordsContainTextWWW(string userId, string text)
        {
            FilterDefinition<WordModel> filter = Builders<WordModel>.Filter.Empty;

           if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(text))
            {
                filter &= Builders<WordModel>.Filter.Where(x => 
                x.UserId==userId &&
                x.Title.ToUpperInvariant().StartsWith(text.ToUpperInvariant()));
            }
            return await _context.Words.Find(filter).ToListAsync();
        }

        public async Task<bool> Create(WordModel word)
        {
            try
            {
                await _context.Words.InsertOneAsync(word);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(string wordId, WordModel updatedWord)
        {
            try
            {
                await _context.Words.ReplaceOneAsync(word => word.WordId == wordId, updatedWord);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                await _context.Words.DeleteOneAsync(u => u.WordId == id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> Like(string userId, string wordId)
        {
            WordModel wordModel = await GetWordById(wordId);
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
                await Update(wordModel.WordId, wordModel);
                return wordModel.Likes.Count();
            }
            else return -1;
        }
        public async Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage)
        {
            List<WordDto> wordDtos = new();
            WordModel word;
            WordDto wordDto;
            int newCurrentPage = currentPage;
            Tuple<int, List<WordDto>> Res;

            long wordsCount = await GetDocCount(userId, lang);

            while (wordDtos?.Count < pageSize)
            {
                if (wordsCount < newCurrentPage)
                {
                    break;
                }
                word = await GetWord(userId, lang, newCurrentPage, 1);
                if (word != null)
                {
                    wordDto = _mapper.Map<WordDto>(word);
                    if (await IsWordSharedWithUser(wordDto, currentUserId))
                    {
                        if (word.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                        if (currentUserId != wordDto?.UserId)
                        {
                            var user = await _userRepo.GetUserById(wordDto.UserId);
                            wordDto.UserName = user?.Name;
                        }
                        wordDtos.Add(wordDto);
                    }
                }
                newCurrentPage++;
            }

            return wordDtos;
        }

        public async Task<bool> IsWordSharedWithUser(WordDto wordDto, string? userId)
        {
            if (userId == wordDto?.UserId)
            {
                return true;
            }

            var relationshipId = await _relationshipRepo.GetRelationshipId(userId, Relation.Friend, wordDto?.UserId);

            if (!string.IsNullOrEmpty(relationshipId))
            {
                return wordDto.ShareWith == ShareWith.Friends || wordDto.ShareWith == ShareWith.Public;
            }
            else
            {
                return wordDto?.ShareWith == ShareWith.Public;
            }
        }

        public async Task<WordDto> GetWordDtoById(string id, string? currentUserId)
        {
            var word = await GetWordById(id);
            var wordDto = _mapper.Map<WordDto>(word);

            if (await IsWordSharedWithUser(wordDto, currentUserId))
            {
                if (word?.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                if (currentUserId != wordDto?.UserId)
                {
                    var user = await _userRepo.GetUserById(wordDto.UserId);
                    wordDto.UserName = user?.Name;
                }
                return wordDto;
            }
            else return null;
        }

        public async Task<WordDto> GetWordByText(string userId, string text)
        {
            var word = await _context.Words.Find<WordModel>(u => u.Title == text && u.UserId == userId).FirstOrDefaultAsync();

            if (word != null)
            {
                word.Score++;
                await Update(word.WordId, word);
            }
            return _mapper.Map<WordDto>(word);
        }

        public async Task<IEnumerable<string>> GetWordsContainText(string userId, string text)
        {
            List<string> res = new();
            var words = await GetWordsContainTextWWW(userId, text);
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
            if (await Create(word)) return word.WordId;
            else return null;
        }

        public async Task<bool> UpdateWord(WordDto updatedWordDto)
        {
            var oldWord = await GetWordById(updatedWordDto.WordId);
            if (oldWord == null) return false;

            var word = _mapper.Map<WordModel>(updatedWordDto);
            word.Comments = oldWord.Comments;
            word.Likes = oldWord.Likes;
            word.CreatedAt = DateTime.Now;
            return await Update(updatedWordDto.WordId, word);
        }

        public async Task<bool> SaveComment(string wordId, CommentModel newComment)
        {
            try
            {
                WordModel word = await GetWordById(wordId);
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
                    await Update(wordId, word);
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
            WordModel word = await GetWordById(wordId);
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
            WordModel word = await GetWordById(wordId);

            CommentModel comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return false;
            if (comment.UserId != currUsr) return false;

            if (word.Comments.Contains(comment))
            {
                word.Comments.Remove(comment);
                await Update(wordId, word);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<CommentModel?>> GetWordComments(string wordId)
        {
            WordModel word = await GetWordById(wordId);
            return word?.Comments;
        }
    }
}
