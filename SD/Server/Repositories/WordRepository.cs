using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
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
            words = words.OrderByDescending(w => w.CreatedAt).ToList();

            if (CurrentUserId == userId)
            {
                return words;
            }
            else
            {
                if (await IsFriendAsync(CurrentUserId, userId))
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

        private async Task<bool> IsFriendAsync(string currentUserId, string userId)
        {
            var user = await _context.Users.Find<UserModel>(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null) return false;
            return user.Friends.Contains(currentUserId);
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
