using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using sd.Api.Application_layer.Interfaces.Repositories;
using sd.Api.Models;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Infrastructure.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly MongodbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepo;
        private readonly IRelationshipRepository _relationshipRepo;

        public WordRepository(MongodbContext mongodbContext, IMapper mapper, ILogger<WordRepository> logger, IUserRepository userRepos, IRelationshipRepository relationshipRepo)
        {
            _context = mongodbContext;
            _mapper = mapper;
            _userRepo = userRepos;
            _relationshipRepo = relationshipRepo;
        }

        public async Task<long> GetDocCount(string userId, string lang)
        {
            var filter = GetFilter(null, userId, lang);
            return await _context.Words.CountDocumentsAsync(filter);
        }

        public async Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit)
        {
            var filter = GetFilter(null, userId, lang);
            var sort = Builders<WordModel>.Sort.Descending("Score").Descending("CreatedAt");
            try
            {
                return await _context.Words.Find(filter).Sort(sort).Skip(currentPage - 1).Limit(limit).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Create(WordModel word)
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

        public async Task<bool> Update(WordModel updatedWord)
        {
            try
            {
                await _context.Words.ReplaceOneAsync(word => word.WordId == updatedWord.WordId, updatedWord);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string id)
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

        public async Task<IEnumerable<WordModel>> GetByCondation(Expression<Func<WordModel, bool>> expression)
        {
            return await _context.Words.Find(expression).ToListAsync();
        }

        private static FilterDefinition<WordModel> GetFilter(string? wordId, string userId, string lang)
        {
            FilterDefinition<WordModel> filter = Builders<WordModel>.Filter.Empty;
            if (wordId != null) filter &= Builders<WordModel>.Filter.Eq(x => x.WordId, wordId);
            if (userId != null && userId != "0") filter &= Builders<WordModel>.Filter.Eq(x => x.UserId, userId);
            if (lang != null) filter &= Builders<WordModel>.Filter.Eq(x => x.ToLang, lang);

            return filter;
        }

        public async Task<WordModel> GetById(string id)
        {
            var cursor = _context.Words.Find(x => x.WordId  == id);
            var res = await cursor.FirstOrDefaultAsync();
            return res;
        }
    }
}
