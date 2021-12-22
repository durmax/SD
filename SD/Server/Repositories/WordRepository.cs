using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Helpers;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System.Collections.Generic;
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

        public async Task<long> GetDocCount(string userId, string lang)
        {
            var filter = WordHelper.GetFilter(null, userId, lang);
            return await _context.Words.CountDocumentsAsync(filter);
        }

        public async Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit)
        {
            var filter = WordHelper.GetFilter(null, userId, lang);
            try
            {
                return await _context.Words.Find(filter).SortByDescending(d => d.CreatedAt).Skip(currentPage - 1).Limit(limit).FirstOrDefaultAsync();
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
        public async Task<WordModel> GetWordByText(string userId, string text)
        {
            return await _context.Words.Find<WordModel>(u => u.Title == text && u.UserId == userId).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<WordModel>> GetWordsContainText(string userId, string text)
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
