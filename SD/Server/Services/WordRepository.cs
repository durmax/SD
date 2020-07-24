using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class WordRepository : IWordRepository
    {
        private MongodbContext _context;

        public WordRepository(IMongodbSettings settings)
        {
            _context = new MongodbContext(settings);
        }

        public async Task<List<WordModel>> GetAllWords(string CurrentUserId, string userId)
        {
            List<WordModel> words = new List<WordModel>();

            words = await _context.Words.Find(w => w.UserId == userId).ToListAsync();
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
          return  user.Friends.Contains(currentUserId);
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

        public async Task<bool> UpdateWord(string id, WordModel updatedWord)
        {
            try
            {
                await _context.Words.ReplaceOneAsync(word => word.WordId == id, updatedWord);
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
