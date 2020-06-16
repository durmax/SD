using Microsoft.Extensions.Options;
using MongoDB.Driver;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class WordRepository : IWordRepository
    {
        private MongodbContext _context;

        public WordRepository(IOptions<MongodbSettings> settings)
        {
            _context = new MongodbContext(settings);
        }

        public async Task<List<WordModel>> GetAllWords(string userId)
        {
            return await _context.Words.Find(w => w.UserId == userId).ToListAsync();
        }
        public async Task<WordModel> GetWordById(string id)
        {
            return await _context.Words.Find(w => w.WordId == id).FirstOrDefaultAsync();
        }
        public async Task<WordModel> GetWordByText(string userId, string text)
        {
            return await _context.Words.Find<WordModel>(u => u.Title == text && u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<WordModel> AddWord(WordModel word)
        {
            await _context.Words.InsertOneAsync(word);
            return word;
        }

        public async Task<WordModel> UpdateWord(string id, WordModel updatedWord)
        {
            await _context.Words.ReplaceOneAsync(word => word.WordId == id, updatedWord);
            return updatedWord;
        }


        public async Task<bool> RemoveWord(string id)
        {
            await _context.Words.DeleteOneAsync(u => u.WordId == id);

            return true;
        }
    }
}
