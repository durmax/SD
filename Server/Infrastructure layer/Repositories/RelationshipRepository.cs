using System.Collections.Generic;
using System.Threading.Tasks;
using sd.Shared;
using sd.Api.Models;
using MongoDB.Driver;
using System;
using sd.Api.Infrastructure.Repositories;
using System.Linq.Expressions;

namespace sd.Api.Repositories
{
    public interface IRelationshipRepository : ICrudBase<RelationshipModel>
    {
    }

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

        public async Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression)
        {
            return await _context.Relationships.Find(expression).ToListAsync();
        }
    }
}