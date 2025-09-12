using AutoMapper;
using sd.Application.Interfaces.Repositories;
using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Services
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
        Task<string> GetAI(string wordTitle);
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

        public async Task<List<WordDto>> GetPageWords(
            string? currentUserId, string userId, string lang, int pageSize, int offset)
        {
            if (pageSize <= 0) return new List<WordDto>();
            if (offset < 0) offset = 0;

            var result = new List<WordDto>(capacity: pageSize);

            // 2) Pull words in batches until we collect pageSize visible items or run out
            const int fetchBatch = 50; // tune as needed
            int cursor = offset;
            var authorCache = new Dictionary<string, UserModel?>();

            while (result.Count < pageSize)
            {
                var batch = await _wordRepo.GetWords(userId, lang, cursor, fetchBatch);
                if (batch.Count == 0) break;

                foreach (var word in batch)
                {
                    var dto = _mapper.Map<WordDto>(word);

                    // Visibility check (inlined logic from IsWordSharedWithUser, but O(1) with friendIds)
                    if (!await IsVisibleToViewerAsync(dto, currentUserId)) continue;

                    if (currentUserId != null && word.Likes != null && word.Likes.Contains(currentUserId))
                        dto.IsILiked = true;

                    if (!string.Equals(currentUserId, dto.UserId, StringComparison.Ordinal))
                    {
                        if (!authorCache.TryGetValue(dto.UserId, out var author))
                        {
                            author = await _userRepo.GetById(dto.UserId);
                            authorCache[dto.UserId] = author;
                        }
                        dto.UserName = author?.Name;
                    }

                    result.Add(dto);
                    if (result.Count == pageSize) break;
                }

                // Advance cursor by how many we *scanned*, not how many we *kept*
                cursor += batch.Count;
            }

            return result;
        }

        private async Task<bool> IsVisibleToViewerAsync(WordDto word, string? viewerId)
        {
            // Owner always sees it
            if (viewerId == word.UserId) return true;

            // Public visible to anyone
            if (word.ShareWith == ShareWith.Public) return true;

            HashSet<string> viewerFriends = viewerId is null
                            ? new HashSet<string>()
                            : await GetFriendIdsForViewer(viewerId);

            // Friends visibility if viewer is a friend of the author
            if (word.ShareWith == ShareWith.Friends && viewerId != null)
                return viewerFriends.Contains(word.UserId);

            return false;
        }

        private async Task<HashSet<string>> GetFriendIdsForViewer(string viewerId)
        {
            var relations = await _relationshipRepo.GetByCondation(x =>
                x.Reletion == Relation.Friend &&
                (x.UserId1 == viewerId || x.UserId2 == viewerId));

            var set = new HashSet<string>();
            foreach (var r in relations)
            {
                if (r.UserId1 == viewerId) set.Add(r.UserId2);
                else if (r.UserId2 == viewerId) set.Add(r.UserId1);
            }
            return set;
        }

        public async Task<WordDto> GetWordDtoById(string id, string? currentUserId)
        {
            var word = await _wordRepo.GetById(id);
            var wordDto = _mapper.Map<WordDto>(word);

            if (!await IsVisibleToViewerAsync(wordDto, currentUserId)) return null;

            if (currentUserId != null && word.Likes != null && word.Likes.Contains(currentUserId))
                wordDto.IsILiked = true;

            if (currentUserId != wordDto?.UserId)
            {
                var user = await _userRepo.GetById(wordDto.UserId);
                wordDto.UserName = user?.Name;
            }
            return wordDto;
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
            if (string.IsNullOrWhiteSpace(wordDto.Title) || string.IsNullOrWhiteSpace(wordDto.UserId))
                return null;

            var word = _mapper.Map<WordModel>(wordDto);
            word.WordId = Guid.NewGuid().ToString();
            word.CreatedAt = DateTime.Now;

            return await _wordRepo.Create(word) ? word.WordId : null;
        }

        public async Task<bool> Update(WordModel word)
        {
            var existing = await _wordRepo.GetById(word.WordId);
            if (existing == null) return false;

            word.Comments = existing.Comments;
            word.Likes = existing.Likes;
            word.CreatedAt = DateTime.Now; // DateTimeOffset.UtcNow; toDo

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
                if (word == null) return false;
                if (word.Comments != null)
                {
                    var comment = word.Comments.SingleOrDefault(x => x.CommentId == newComment.CommentId);
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
            catch
            {
                return false;
            }
        }

        public async Task<int> LikeComment(string userId, string wordId, string commentId)
        {
            var word = await _wordRepo.GetById(wordId);
            if (word?.Comments == null) return 0;

            var comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return 0;

            comment.Likes ??= new List<string>();
            if (!comment.Likes.Contains(userId))
                comment.Likes.Add(userId);
            else
                comment.Likes.Remove(userId);

            var ok = await _wordRepo.Update(word);
            return ok ? comment.Likes.Count : 0;
        }

        public async Task<bool> DeleteComment(string currUsr, string wordId, string commentId)
        {
            var word = await _wordRepo.GetById(wordId);
            if (word?.Comments == null) return false;

            var comment = word.Comments.SingleOrDefault(x => x.CommentId == commentId);
            if (comment == null) return false;

            if (comment.UserId != currUsr /* && !IsModerator(currUsr) */) return false;

            word.Comments.Remove(comment);
            return await _wordRepo.Update(word);
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
            if (string.IsNullOrWhiteSpace(wordTitle)) return string.Empty;
            var prompt = $"Schreibe Beispiele auf Niveau B1, die mir helfen, die Bedeutungen von „{wordTitle}“ zu verstehen.";
            return await _geminiService.ProcessStringAsync(prompt);
        }

        public async Task<WordModel> GetById(string id)
        {
            return await _wordRepo.GetById(id);
        }
    }
}
