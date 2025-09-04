using MongoDB.Driver;
using sd.Api.Models;
using sd.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Linq.Expressions;
using System;
using sd.Api.Application_layer.Interfaces.Repositories;

namespace sd.Api.Infrastructure.Repositories
{
    public class OtherPageRepository : IOtherPageRepository
    {
        private readonly MongodbContext _context = null;

        public OtherPageRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<bool> Create(OtherPageModel otherPage)
        {
            return await _context.OtherPages.Find<OtherPageModel>(u => u.Host == otherPage.Host).AnyAsync();
        }

        public async Task<bool> Update(OtherPageModel newOtherPage)
        {
            try
            {
                await _context.OtherPages.FindOneAndReplaceAsync(Builders<OtherPageModel>.Filter.Eq("OtherPageId", newOtherPage.OtherPageId), newOtherPage);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string id)
        {
            var DeleteRecored = await _context.OtherPages.DeleteOneAsync(
              Builders<OtherPageModel>.Filter.Eq("OtherPageId", id));
            return DeleteRecored.DeletedCount > 0;
        }

        public async Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression)
        {
            return await _context.OtherPages.AsQueryable().Where(expression).ToListAsync();
        }

        public async Task<OtherPageModel?> GetById(string id)
        {
            var cursor = _context.OtherPages.Find(u => u.OtherPageId == id);
            var res = await cursor.FirstOrDefaultAsync();
            return res;
        }
    }
}