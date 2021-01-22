using sd.Api.Interfaces;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class LikeWordService : ILikeWordService
    {
        private IWordRepository _wordRepository;
        public LikeWordService(IWordRepository wordRepository)
        {
            _wordRepository = wordRepository;
        }

        public async Task<int> Like(string userId, string wordId)
        {
            WordModel wordModel = await _wordRepository.GetWordById(wordId);
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
                await _wordRepository.UpdateWord(wordModel.WordId, wordModel);
                return wordModel.Likes.Count();
            }
            else return -1;
        }
    }
}
