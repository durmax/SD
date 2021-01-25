using MongoDB.Driver;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Linq;

namespace sd.Api.Repositories
{
    public class OtherPageRepository : IOtherPageRepository
    {
        private readonly MongodbContext _context = null;

        public OtherPageRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<IEnumerable<OtherPageResModel>> GetOPResModels(string fromLang, string toLang)
        {
            List<OtherPageResModel> res = new List<OtherPageResModel>();

            var otherPages = await FilterByLangs(fromLang, toLang);

            //otherPages.Sort((x, y) => x.Order.CompareTo(y.Order));

            if (otherPages != null)
            {
                foreach (var otherPage in otherPages)
                {
                    if (!string.IsNullOrEmpty(otherPage.Pattern))
                    {
                        OtherPageResModel otherPageResModel = new OtherPageResModel();
                        otherPageResModel.Pattern = otherPage.Pattern;
                        otherPageResModel.Host = otherPage.Host;
                        otherPageResModel.Type = otherPage.PageType;
                        otherPageResModel.Eval = otherPage.Eval;
                        res.Add(otherPageResModel);
                    }
                }
                // return res;
            }
             
            return res.OrderBy(o=> o.Eval);
        }

        public async Task<OtherPageModel> GetOtherPageById(string id)
        {
            return await _context.OtherPages.Find<OtherPageModel>(u => u.OtherPageId == id).FirstOrDefaultAsync();
        }
        private async Task<IEnumerable<OtherPageModel>> FilterByLangs(string fromLang, string toLang)
        {

            return await _context.OtherPages.AsQueryable<OtherPageModel>()
               .Where(o =>
                  (o.PrimLangs == "All" && o.SecLangs == "All")
               || (o.PrimLangs.Contains(fromLang) && (o.SecLangs.Contains(toLang) || o.SecLangs == "All"))
               || (o.PrimLangs.Contains(toLang) && o.SecLangs.Contains(fromLang))
                   ).ToListAsync();
        }

        public async Task<TransObj> RegisterOtherPage(OtherPageModel otherPage)
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

        public async Task<TransObj> UpdateOtherPage(string id, OtherPageModel newOtherPage)
        {
            try
            {
                await _context.OtherPages.FindOneAndReplaceAsync(
      Builders<OtherPageModel>.Filter.Eq("OtherPageId", id), newOtherPage);

            }
            catch
            {
                return new TransObj { BoolVar = false, SetringVar = "Sorry, update data error" };
            }

            return new TransObj { BoolVar = true, SetringVar = "Your data updated successfully" };
        }

        public async Task<bool> RemoveOtherPage(string id)
        {
            DeleteResult DeleteRecored;
            DeleteRecored = await _context.OtherPages.DeleteOneAsync(
              Builders<OtherPageModel>.Filter.Eq("OtherPageId", id));
            return DeleteRecored.IsAcknowledged;
        }
    }
}
