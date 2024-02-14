using System.Collections.Generic;
using sd.Api.Interfaces;
using System.Threading.Tasks;
using SD.Shared;
using sd.Api.Models;
using MongoDB.Driver;

namespace sd.Api.Repositories
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
            try
            {
                await _context.Relationships.InsertOneAsync(relationship);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Updat(string oldRelationshipId, RelationshipModel newRelationship)
        {
            await _context.Relationships.FindOneAndReplaceAsync(r => r.RelationshipId == oldRelationshipId, newRelationship);
            return true;
        }
        public async Task<bool> Delete(string oldRelationshipId)
        {
            await _context.Relationships.DeleteOneAsync(r => r.RelationshipId == oldRelationshipId);
            return true;
        }

        public async Task<List<RelationshipModel>> GetAllFrindsRelationships(string userId)
        {
            return await _context.Relationships.Find<RelationshipModel>(r => (r.UserId1 == userId || r.UserId2 == userId) && r.Reletion == Relation.Friend).ToListAsync();
        }

        public async Task<List<RelationshipModel>> FriendRequestsToUser(string userId)
        {
            return await _context.Relationships.Find<RelationshipModel>(r => r.UserId2 == userId && r.Reletion == Relation.FriendRequestTo).ToListAsync();
        }

        public async Task<RelationshipModel> GetRelationshipById(string id)
        {
            return await _context.Relationships.Find<RelationshipModel>(r => r.RelationshipId == id).FirstOrDefaultAsync();
        }

        private FilterDefinition<RelationshipModel> GetFilter(string userId1, Relation reletion, string userId2)
        {
            FilterDefinition<RelationshipModel> filter = Builders<RelationshipModel>.Filter.Empty;

            if (reletion != Relation.None) filter &= Builders<RelationshipModel>.Filter.Eq(x => x.Reletion, reletion);

            FilterDefinition<RelationshipModel> Case1 = Builders<RelationshipModel>.Filter.Eq(x => x.UserId1, userId1);
            Case1 &= Builders<RelationshipModel>.Filter.Eq(x => x.UserId2, userId2);

            FilterDefinition<RelationshipModel> Case2 = Builders<RelationshipModel>.Filter.Eq(x => x.UserId1, userId2);
            Case2 &= Builders<RelationshipModel>.Filter.Eq(x => x.UserId2, userId1);

            filter &= (Case1 |= Case2);

            return filter;
        }
        public async Task<string> GetRelationshipId(string userId1, Relation relation, string userId2)
        {
            RelationshipModel relationship = await _context.Relationships.Find(GetFilter(userId1, relation, userId2)).FirstOrDefaultAsync();
            return relationship?.RelationshipId;
        }

        public async Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2)
        {
            RelationshipModel relationship = await _context.Relationships.Find(GetFilter(userId1, Relation.None, userId2)).FirstOrDefaultAsync();
            if (relationship == null) return Relation.None;
            return relationship.Reletion;

        }
    }
}
