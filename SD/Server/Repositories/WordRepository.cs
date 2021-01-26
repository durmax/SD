using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly MongodbContext _context;

        public WordRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<List<WordModel>> GetAllWords(string CurrentUserId, string userId, int pageSize, int currentPage)
        {
            List<WordModel> words = new List<WordModel>();
            words = await _context.Words.Find(w => w.UserId == userId).SortByDescending(d => d.CreatedAt).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToListAsync();
            //words = words.OrderByDescending(w => w.CreatedAt).ToList();

            if (CurrentUserId == userId)
            {
                return words;
            }
            else
            {
                if (await AreFriendAsync(CurrentUserId, userId))
                {
                    words = words.Where(w => w.ShareWith > 0).Select(w => w).ToList();
                }
                else
                {
                    words = words.Where(w => w.ShareWith > 1).Select(w => w).ToList();
                }
            }
            return words;
        }

        private async Task<bool> AreFriendAsync(string currentUserId, string userId)
        {
            var user = await _context.Users.Find<UserModel>(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null || user.Friends == null) return false;
            return (bool)(user.Friends?.Contains(currentUserId));
        }

        private async Task<bool> IsWordShareAsync(WordModel word, string currentUserId)
        {
            bool areSame = currentUserId == word.UserId ? true : false;

            if (areSame)
            {
                return true;
            }

            else
            {
                if (await AreFriendAsync(currentUserId, word.UserId))
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
        
        public async Task<Tuple<int, List<WordModel>>> GetWords(string currentUserId, string lang, int pageSize, int currentPage)
        {
            List<WordModel> words = new List<WordModel>();
            int newCurrentPage = currentPage;
            Tuple<int, List<WordModel>> Res;

            while (words.Count < pageSize)
            {
                WordModel word = await GetNextWordAsync(newCurrentPage, currentUserId, lang);
                if (word != null)
                {
                    newCurrentPage++;
                    words.Add(word);
                }
            }

            Res = new Tuple<int, List<WordModel>>(newCurrentPage, words);

            return Res;
        }

        private async Task<WordModel> GetNextWordAsync(int currentPage,string currentUserId, string lang)
        {
            WordModel word;
            word = await _context.Words.Find(w => w.WordLang == lang).SortByDescending(d => d.CreatedAt).Skip(currentPage - 1).Limit(1).FirstOrDefaultAsync();

            if (await IsWordShareAsync(word, currentUserId)) return word;
            return null;
        }

        public async Task<WordModel> GetWordById(string id)
        {
            return await _context.Words.Find(w => w.WordId == id).FirstOrDefaultAsync();
        }
        public async Task<WordModel> GetWordByText(string userId, string text)
        {
            return await _context.Words.Find<WordModel>(u => u.Title == text && u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<bool> AddWord(WordModel word)
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

        public async Task<bool> UpdateWord(string wordId, WordModel updatedWord)
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

        public async Task<bool> RemoveWord(string id)
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
    }
}
