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
        private readonly IMapper _mapper;

        public WordService(IWordRepository wordRepository, RelationshipService relationshipService, IMapper mapper)
        {
            _wordRepository = wordRepository;
            _relationshipService = relationshipService;
            _mapper = mapper;
        }

        public async Task<Tuple<int, List<WordDto>>> GetPageWords(string currentUserId, string userId, string lang, int pageSize, int currentPage)
        {
            List<WordDto> words = new List<WordDto>();
            WordModel word;
            WordDto wordDto;
            int newCurrentPage = currentPage;
            Tuple<int, List<WordDto>> Res;

            long wordsCount = await _wordRepository.GetDocCount(userId, lang);

            while (words.Count < pageSize)
            {
                if (wordsCount < newCurrentPage)
                {
                    break;
                }
                word = await _wordRepository.GetWord(userId, lang, newCurrentPage, 1);
                if (word != null)
                {
                    wordDto = _mapper.Map<WordDto>(word);
                    if (await IsWordShareWithUser(wordDto, currentUserId))
                    {
                        if (word.Likes!=null && word.Likes.Contains(currentUserId)) wordDto.IsILiked = true;
                        words.Add(wordDto);
                    }
                }
                newCurrentPage++;
            }
            
            Res = new Tuple<int, List<WordDto>>(newCurrentPage, words);

            return Res;
        }

        private async Task<bool> IsWordShareWithUser(WordDto wordDto, string userId)
        {
            var word = _mapper.Map<WordModel>(wordDto);
            bool areSame = userId == word.UserId ? true : false;

            if (areSame)
            {
                return true;
            }

            else
            {
                if (await _relationshipService.AreFrinds(userId, word.UserId) != "0")
                {
                    if (word.ShareWith > 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (word.ShareWith > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<WordDto> GetWordDtoById(string id)
        {
            var word = await _wordRepository.GetWordById(id);
            return _mapper.Map<WordDto>(word);
        }
        public async Task<WordModel> GetWordById(string id)
        {
           return await _wordRepository.GetWordById(id);
             
        }

        public async Task<WordDto> GetWordByText(string userId, string text)
        {
            var word = await _wordRepository.GetWordByText(userId, text);
            return _mapper.Map<WordDto>(word);
        }

        public async Task<IEnumerable<string>> GetWordsContainText(string userId, string text)
        {
            List<string> res = new List<string>();
            var words = await _wordRepository.GetWordsContainText(userId, text);
            foreach (var w in words)
            {
                res.Add(w.Title);
            }
            return res;
        }

        public async Task<bool> AddWord(WordDto wordDto)
        {
            var word = _mapper.Map<WordModel>(wordDto);
            word.CreatedAt = DateTime.Now;
            return await _wordRepository.AddWord(word);
        }

        public async Task<bool> UpdateWord(WordDto updatedWordDto)
        {     
            var oldWord = await _wordRepository.GetWordById(updatedWordDto.WordId);
            if (oldWord == null) return false;
            
            var word = _mapper.Map<WordModel>(updatedWordDto);
            word.Comments = oldWord.Comments;
            word.Likes = oldWord.Likes;
            word.CreatedAt = DateTime.Now;
            return await _wordRepository.UpdateWord(updatedWordDto.WordId, word);
        }

        public async Task<bool> RemoveWord(string id)
        {
            return await _wordRepository.RemoveWord(id);
        }
    }
}
