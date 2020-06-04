using Microsoft.Extensions.Options;
using MongoDB.Driver;
using sd.Api.Interfaces;
using sd.Api.Models;
using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace sd.Api.Services
{
    public class OtherPageRepository : IOtherPageRepository
    {
        private readonly MongodbContext _context = null;

        public OtherPageRepository(IOptions<MongodbSettings> settings)
        {
            _context = new MongodbContext(settings);
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

            return res;
        }

        public async Task<OtherPageModel> GetOtherPageById(string id)
        {
            return await _context.OtherPages.Find<OtherPageModel>(u => u.OtherPageId == id).FirstOrDefaultAsync();
        }
        async Task<IEnumerable<OtherPageModel>> FilterByLangs(string fromLang, string toLang)
        {
           // var DictBuilder = Builders<OtherPageModel>.Filter;
           // var DictFilter =

           //     (DictBuilder.ElemMatch<OtherPageModel>("PrimLangs", fromLang)) ||(DictBuilder.ElemMatch<OtherPageModel> ("PrimLangs", fromLang)) 
                
           //     ;

           //     //(DictBuilder.Matches("PrimLangs", fromLang) & DictBuilder.Matches("SecLangs", toLang))
           //     //| (DictBuilder.AnyEq(f => f.PrimLangs, toLang) & DictBuilder.AnyEq(f => f.SecLangs, fromLang))
           //     //| DictBuilder.AnyEq(f => f.PrimLangs, "All");


           // var filter = DictFilter;// | VokFilter;

           //// return await _context.OtherPages.Find(filter).ToListAsync();

            return await _context.OtherPages.AsQueryable<OtherPageModel>()
               .Where(o =>
                  o.PrimLangs == "All"
               || o.SecLangs == "All"
               || o.PrimLangs.Contains(fromLang)
               || o.PrimLangs.Contains(toLang)
               || o.SecLangs.Contains(fromLang)
               || o.SecLangs.Contains(toLang)
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
