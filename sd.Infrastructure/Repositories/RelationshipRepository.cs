using sd.Shared;
using sd.Infrastructure.Models;
using MongoDB.Driver;
using System.Linq.Expressions;
using sd.Application.Interfaces.Repositories;

namespace sd.Infrastructure.Repositories
{
    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly MongodbContext _context;

        public RelationshipRepository(MongodbContext mongodbContext)
        {
            _context = mongodbContext;
        }

        public async Task<bool> Create(RelationshipModel relationship)
        {
            await _context.Relationships.InsertOneAsync(relationship);
            return true;
        }
        public async Task<bool> Update(RelationshipModel newRelationship)
        {
            await _context.Relationships.FindOneAndReplaceAsync(r => r.RelationshipId == newRelationship.RelationshipId, newRelationship);
            return true;
        }
        public async Task<bool> Delete(string oldRelationshipId)
        {
            await _context.Relationships.DeleteOneAsync(r => r.RelationshipId == oldRelationshipId);
            return true;
        }

        public async Task<IEnumerable<RelationshipModel>> GetByCondition(Expression<Func<RelationshipModel, bool>> expression)
        {
            return await _context.Relationships.Find(expression).ToListAsync();
        }

        public async Task<RelationshipModel> GetById(string id)
        {
            var cursor = _context.Relationships.Find(x => x.RelationshipId == id);
            var res = await cursor.FirstOrDefaultAsync();
            return res;
        }
    }
}