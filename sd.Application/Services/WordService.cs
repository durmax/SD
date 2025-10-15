using AutoMapper;
using sd.Application.Interfaces.Repositories;
using sd.Application.Services.Gemini;
using sd.Shared;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace sd.Application.Services
{
    public interface IWordService
    {
        Task<IEnumerable<WordModel>> GetByCondition(Expression<Func<WordModel, bool>> expression);
        Task<WordModel> GetById(string id);
        Task<bool> Update(WordModel entity);
        Task<bool> Delete(string id);
        Task<int> Like(string userId, string wordId);
        Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage, CancellationToken ct);
        Task<WordDto?> GetWordDtoById(string id, string? currentUserId);
        Task<WordDto> GetWordByText(string userId, string text);
        Task<IEnumerable<string>> GetWordsContainText(string userId, string text);
        Task<string?> AddWord(WordDto wordDto);
        Task<IEnumerable<CommentModel>> GetWordComments(string wordId);
        Task<bool> SaveComment(string wordId, CommentModel newComment);
        Task<int> LikeComment(string userId, string wordId, string commentId);
        Task<bool> DeleteComment(string currUsr, string wordId, string commentId);
        Task<string> GetWordMeaningAI(string wordTitle, CancellationToken cancellationToken);
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
             string? currentUserId,
             string userId,
             string lang,
             int pageSize,
             int offset,
             CancellationToken ct = default)
        {
            if (pageSize <= 0) return new List<WordDto>();
            if (offset < 0) offset = 0;

            var result = new List<WordDto>(capacity: pageSize);

            const int fetchBatch = 50; // tune
            const int scanCapMultiplier = 10; // don’t scan forever
            int cursor = offset;
            int scanned = 0;
            int scanCap = Math.Max(pageSize * scanCapMultiplier, fetchBatch);

            var authorCache = new Dictionary<string, UserModel?>();

            HashSet<string> viewerFriends = currentUserId is null
                ? new HashSet<string>()
                : await GetFriendIdsForViewer(currentUserId);

            while (result.Count < pageSize)
            {
                ct.ThrowIfCancellationRequested();

                var batch = await _wordRepo.GetWords(userId, lang, cursor, fetchBatch);
                if (batch.Count == 0) break;

                foreach (var word in batch)
                {
                    // Visibility check (inlined logic from IsWordSharedWithUser, but O(1) with friendIds)
                    if (!IsVisibleToViewerAsync(word, currentUserId, viewerFriends)) continue;

                    var dto = _mapper.Map<WordDto>(word);

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

                if (scanned >= scanCap) break; // safety stop
            }

            return result;
        }

        private static bool IsVisibleToViewerAsync(WordModel word, string? viewerId, HashSet<string> viewerFriends)
        {
            // Owner always sees it
            if (viewerId == word.UserId) return true;

            // Public visible to anyone
            if (word.ShareWith == (int) ShareWith.Public) return true;

            // Friends visibility if viewer is a friend of the author
            if (word.ShareWith == (int) ShareWith.Friends && viewerId != null)
                return viewerFriends.Contains(word.UserId);

            return false;
        }

        private async Task<HashSet<string>> GetFriendIdsForViewer(string viewerId)
        {
            var relations = await _relationshipRepo.GetByCondition(x =>
                x.Relation == Relation.Friend &&
                (x.UserId1 == viewerId || x.UserId2 == viewerId));

            var set = new HashSet<string>();
            foreach (var r in relations)
            {
                if (r.UserId1 == viewerId) set.Add(r.UserId2);
                else if (r.UserId2 == viewerId) set.Add(r.UserId1);
            }
            return set;
        }

        public async Task<WordDto?> GetWordDtoById(string id, string? currentUserId)
        {
            var word = await _wordRepo.GetById(id);

            HashSet<string> viewerFriends = currentUserId is null
                            ? new HashSet<string>()
                            : await GetFriendIdsForViewer(currentUserId);

            if (!IsVisibleToViewerAsync(word, currentUserId, viewerFriends)) return null;

            var wordDto = _mapper.Map<WordDto>(word);

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
            var words = await _wordRepo.GetByCondition(u => string.Equals(u.Title, text, StringComparison.OrdinalIgnoreCase) && u.UserId == userId);
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
            var words = await _wordRepo.GetByCondition(u => u.Title.ToUpperInvariant().StartsWith(text.ToUpperInvariant()) && u.UserId == userId);

            foreach (var w in words)
            {
                res.Add($"{w.Title}:{w.WordId}");
            }
            return res;
        }

        public async Task<string?> AddWord(WordDto wordDto)
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

        public async Task<IEnumerable<CommentModel>> GetWordComments(string wordId)
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

        public async Task<IEnumerable<WordModel>> GetByCondition(Expression<Func<WordModel, bool>> expression)
        {
            return await _wordRepo.GetByCondition(expression);
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

        public async Task<string> GetWordMeaningAI(string wordTitle, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(wordTitle)) return string.Empty;
            var prompt = $"Ich lerne Deutsch als Fremdsprache auf dem Niveau B1. Erkläre mir die Bedeutung von „{wordTitle}” und schreibe Beispiele, die mir helfen es zu verstehen. Beginne die Antwort direkt mit den Beispielen, ohne einen einleitenden Satz oder eine Begrüßung.";
            var res = await _geminiService.ProcessStringAsync(prompt, cancellationToken);

            res = ConvertFormattingToHtml(res);

            return "------------------------- KI Erklärung ------------------------- </br><h3>" + res + "</h3></br> ------------------------- Ende KI Erklärung -------------------------";
        }

        /// <summary>
        /// Konvertiert gängige Markdown-Symbole (** und *) in entsprechende HTML-Tags (strong und em).
        /// Die Konvertierung von ** (fett) muss vor * (kursiv) erfolgen, um Fehler zu vermeiden.
        /// </summary>
        /// <param name="markdownText">Der Eingabetext, der Markdown-Symbole enthält (z.B. von der Gemini API). Das ist der Text, den du mir als Beispiel gegeben hast.</param>
        /// <returns>Ein String mit HTML-Tags anstelle der Markdown-Symbole.</returns>
        public static string ConvertFormattingToHtml(string markdownText)
        {
            if (string.IsNullOrEmpty(markdownText))
            {
                return markdownText;
            }

            string htmlText = markdownText;

            // 1. **Fette (Strong) Formatierung konvertieren**
            // Suchmuster: \*\*([^\*]+)\*\*
            // Erklärung: Sucht nach Text, der von doppelten Sternchen (**) umschlossen ist.
            // [^\*]+ stellt sicher, dass alles *außer* einem Sternchen erfasst wird, 
            // bis die schließenden ** gefunden werden (nicht-gierig).
            // $1 ist die erfasste Gruppe (der Text zwischen den Symbolen).
            htmlText = Regex.Replace(htmlText, @"\*\*([^\*]+)\*\*", "<strong>$1</strong>");


            // 2. *Kursive (Emphasis) Formatierung konvertieren*
            // Suchmuster: \*([^\*]+)\*
            // Erklärung: Sucht nach Text, der von einzelnen Sternchen (*) umschlossen ist.
            // Wichtig: Da wir ** bereits in Schritt 1 behandelt haben, stellt dieses Muster sicher, 
            // dass nur die einfachen * erfasst werden.
            htmlText = Regex.Replace(htmlText, @"\*([^\*]+)\*", "<em>$1</em>");

            // Hinweis: Listenpunkte (*) und Zeilenumbrüche werden hier nicht in HTML-Listen (<ul>, <li>) 
            // umgewandelt. Dafür wäre eine komplexere Logik oder eine dedizierte Markdown-Bibliothek nötig.
            // Der Einfachheit halber belassen wir die Listenelemente als Klartext.

            return htmlText;
        }

        public async Task<WordModel> GetById(string id)
        {
            return await _wordRepo.GetById(id);
        }
    }
}
