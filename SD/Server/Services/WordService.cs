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
        private readonly IUserRepository _userRepository;

        public WordService(IWordRepository wordRepository, IUserRepository userRepository)
        {
            _wordRepository = wordRepository;
            _userRepository = userRepository;
        }

        public async Task<Tuple<int, List<WordModel>>> GetPageWords(string currentUserId, string userId, string lang, int pageSize, int currentPage)
        {
            List<WordModel> words = new List<WordModel>();
            WordModel word;
            int newCurrentPage = currentPage;
            Tuple<int, List<WordModel>> Res;

            long wordsCount = await _wordRepository.GetDocCount(userId, lang);

            while (words.Count < pageSize)
            {
                if (wordsCount < newCurrentPage )
                {
                    break;
                }
                word = await _wordRepository.GetWord(userId, lang, newCurrentPage, 1);
                if (word!= null)
                {
                    if (await IsWordShareWithUser(word, currentUserId)) words.Add(word);
                }
                
                newCurrentPage++;

            }

            Res = new Tuple<int, List<WordModel>>(newCurrentPage, words);

            return Res;
        }

        //public async Task<Tuple<int, List<WordModel>>> GetWords(string currentUserId, string lang, int pageSize, int currentPage)
        //{
        //    List<WordModel> words = new List<WordModel>();
        //    int newCurrentPage = currentPage;
        //    Tuple<int, List<WordModel>> Res;

        //    while (words.Count < pageSize)
        //    {
        //        WordModel word = await _wordRepository.GetWord(currentUserId, lang, newCurrentPage, 10);
        //        newCurrentPage++;
        //        if (word != null)
        //        {
        //            words.Add(word);
        //        }
        //    }

        //    Res = new Tuple<int, List<WordModel>>(newCurrentPage, words);

        //    return Res;
        //}

        private async Task<bool> IsWordShareWithUser(WordModel word, string userId)
        {
            bool areSame = userId == word.UserId ? true : false;

            if (areSame)
            {
                return true;
            }

            else
            {
                if (await AreFriendAsync(userId, word.UserId))
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

        private async Task<bool> AreFriendAsync(string currentUserId, string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null || user.Friends == null) return false;
            return (bool)(user.Friends?.Contains(currentUserId));
        }

        public async Task<WordModel> GetWordById(string id)
        {
          return await _wordRepository.GetWordById(id);
        }

        public async Task<WordModel> GetWordByText(string userId, string text)
        {
            return await _wordRepository.GetWordByText(userId, text);
        }

        public async Task<bool> AddWord(WordModel word)
        {
            return await _wordRepository.AddWord(word);
        }


        public async Task<bool> UpdateWord(string wordId, WordModel updatedWord)
        {
            return await _wordRepository.UpdateWord(wordId,  updatedWord);
        }


        public async Task<bool> RemoveWord(string id)
        {
            return await _wordRepository.RemoveWord(id);
        }

        }
}
