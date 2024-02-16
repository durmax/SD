using AutoMapper;
using sd.Api.Interfaces;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class WordService
    {
        private readonly IWordRepository _wordRepository;
        private readonly RelationshipService _relationshipService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public WordService(IWordRepository wordRepository, RelationshipService relationshipService, IUserRepository userRepository, IMapper mapper)
        {
            _wordRepository = wordRepository;
            _relationshipService = relationshipService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<WordDto>> GetPageWords(string? currentUserId, string userId, string lang, int pageSize, int currentPage)
        {
            List<WordDto> wordDtos = new();
            WordModel word;
            WordDto wordDto;
            int newCurrentPage = currentPage;
            Tuple<int, List<WordDto>> Res;

            long wordsCount = await _wordRepository.GetDocCount(userId, lang);

            while (wordDtos.Count < pageSize)
            {
                if (wordsCount < newCurrentPage)
                {
                    break;
                }
                word = await _wordRepository.GetWord(userId, lang, newCurrentPage, 1);
                if (word != null)
                {
                    wordDto = _mapper.Map<WordDto>(word);
                    if (await IsWordSharedWithUser(wordDto, currentUserId))
                    {
                        if (word.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                        if (currentUserId != wordDto.UserId)
                        {
                            var user = await _userRepository.GetUserById(wordDto.UserId);
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
            if (userId == wordDto.UserId)
            {
                return true;
            }

            var relationshipId = await _relationshipService.GetRelationshipId(userId, Relation.Friend, wordDto.UserId);

            if (!string.IsNullOrEmpty(relationshipId))
            {
                return wordDto.ShareWith == ShareWith.Friends || wordDto.ShareWith == ShareWith.Public;
            }
            else
            {
                return wordDto.ShareWith == ShareWith.Public;
            }
        }

        public async Task<WordDto> GetWordDtoById(string id, string? currentUserId)
        {
            var word = await _wordRepository.GetWordById(id);
            var wordDto = _mapper.Map<WordDto>(word);

            if (await IsWordSharedWithUser(wordDto, currentUserId))
            {
                if (word.Likes != null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                if (currentUserId != wordDto.UserId)
                {
                    var user = await _userRepository.GetUserById(wordDto.UserId);
                    wordDto.UserName = user?.Name;
                }
                return wordDto;
            }
            else return null; 
        }
        public async Task<WordModel> GetWordById(string id)
        {
            return await _wordRepository.GetWordById(id);

        }

        public async Task<WordDto> GetWordByText(string userId, string text)
        {
            var word = await _wordRepository.GetWordByText(userId, text);

            if (word != null)
            {
                word.Score++;
                await _wordRepository.Update(word.WordId, word);
            }
            return _mapper.Map<WordDto>(word);
        }

        public async Task<IEnumerable<string>> GetWordsContainText(string userId, string text)
        {
            List<string> res = new();
            var words = await _wordRepository.GetWordsContainText(userId, text);
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
            if (await _wordRepository.Create(word)) return word.WordId;
            else return null;
        }

        public async Task<bool> UpdateWord(WordDto updatedWordDto)
        {
            var oldWord = await _wordRepository.GetWordById(updatedWordDto.WordId);
            if (oldWord == null) return false;

            var word = _mapper.Map<WordModel>(updatedWordDto);
            word.Comments = oldWord.Comments;
            word.Likes = oldWord.Likes;
            word.CreatedAt = DateTime.Now;
            return await _wordRepository.Update(updatedWordDto.WordId, word);
        }

        public async Task<bool> RemoveWord(string id)
        {
            return await _wordRepository.Delete(id);
        }
    }
}
