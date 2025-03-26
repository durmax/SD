using MongoDB.Driver;
using sd.Api.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IOtherPageRepository : ICrudBase<OtherPageModel>
    {
    }

    public class OtherPageRepository : IOtherPageRepository
    {
        private readonly MongodbContext _context = null;

        public OtherPageRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<TransObj> Create(OtherPageModel otherPage)
        {
            // Add custom model validation error
            bool IsExist = await _context.OtherPages.Find<OtherPageModel>(u => u.Host == otherPage.Host).AnyAsync();

            if (!IsExist)
            {
                await _context.OtherPages.InsertOneAsync(otherPage);
                return new TransObj
                { BoolVar = true, SetringVar = "User Details Inserted Successfully" };
            }
            else
            {
                return new TransObj { BoolVar = false, SetringVar = $"Sorry, this Host is already in DB." };
            }
        }

        public async Task<bool> Update(OtherPageModel newOtherPage)
        {
            try
            {
                await _context.OtherPages.FindOneAndReplaceAsync(
                            Builders<OtherPageModel>.Filter.Eq("OtherPageId", newOtherPage.OtherPageId), newOtherPage);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string id)
        {
            DeleteResult DeleteRecored;
            DeleteRecored = await _context.OtherPages.DeleteOneAsync(
              Builders<OtherPageModel>.Filter.Eq("OtherPageId", id));
            return DeleteRecored.DeletedCount > 0;
        }

        public async Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression)
        {
            return await _context.OtherPages.AsQueryable().Where(expression).ToListAsync();
        }

        Task<bool> ICrudBase<OtherPageModel>.Create(OtherPageModel entity)
        {
            throw new NotImplementedException();
        }
    }
}