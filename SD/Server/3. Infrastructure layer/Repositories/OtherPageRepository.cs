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
        Task<List<OtherPageModel>> FilterByLangs(string fromLang, string toLang);

        Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang);
    }

    public class OtherPageRepository : IOtherPageRepository
    {
        private readonly MongodbContext _context = null;

        public OtherPageRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<List<OtherPageModel>> FilterByLangs(string fromLang, string toLang)
        {

            return await _context.OtherPages.AsQueryable<OtherPageModel>()
               .Where(o =>
                  (o.PrimLangs == "All" && o.SecLangs == "All")
               || (o.PrimLangs.Contains(fromLang) && (o.SecLangs.Contains(toLang) || o.SecLangs == "All"))
               || (o.PrimLangs.Contains(toLang) && o.SecLangs.Contains(fromLang))
                   ).ToListAsync();
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

        public async Task<List<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {
            List<OtherPageResModel> res = new List<OtherPageResModel>();
            var otherPages = await FilterByLangs(fromLang, toLang);

            if (otherPages != null)
            {
                int i = 0;
                foreach (var otherPage in otherPages)
                {
                    if (!string.IsNullOrEmpty(otherPage.Pattern))
                    {
                        OtherPageResModel otherPageResModel = new OtherPageResModel();
                        otherPageResModel.Pattern = otherPage.Pattern;
                        otherPageResModel.Host = otherPage.Host;
                        otherPageResModel.Type = otherPage.PageType;
                        otherPageResModel.Eval = otherPage.Eval == 0 ? i++ : otherPage.Eval;
                        res.Add(otherPageResModel);
                    }
                }
                // return res;
            }
            return res.OrderBy(o => o.Eval).ToList();
        }

        public async Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression)
        {
            return await _context.OtherPages.Find(expression).ToListAsync();
        }

        Task<bool> ICrudBase<OtherPageModel>.Create(OtherPageModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
