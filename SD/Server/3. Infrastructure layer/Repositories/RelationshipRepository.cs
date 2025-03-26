using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using sd.Api.Models;
using MongoDB.Driver;
using System;
using sd.Api.Infrastructure.Repositories;
using System.Linq;
using System.Linq.Expressions;

namespace sd.Api.Repositories
{
    public interface IRelationshipRepository : ICrudBase<RelationshipModel>
    {
        Task<string> GetRelationshipId(string UserId1, Relation reletion, string UserId2);


        Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2);

        Task<bool> AddRelationship(RelationshipModel relationship);
        Task<bool> RemoveFriendship(string UserId, string friendId);
        Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string CurrentUserId, List<UserModel> users);
        Task<Dictionary<string, string>> FriendRequestsToUser(string userId);
    }

    public class RelationshipRepository : IRelationshipRepository
    {
        private readonly MongodbContext _context;
        private readonly IUserRepository _userRepo;

        public RelationshipRepository(MongodbContext mongodbContext, IUserRepository userRepo)
        {
            _context = mongodbContext;
            _userRepo = userRepo;
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
            return relationship == null ? Relation.None : relationship.Reletion;
        }

        public async Task<bool> AddRelationship(RelationshipModel relationship)
        {
            bool res = false;
            Relation relation = await GetRelationshipsBetweenTwoUsers(relationship.UserId1, relationship.UserId2);
            if (relation == Relation.None)
            {
                res = await Create(relationship);
            }
            return res;
        }
        public async Task<bool> RemoveFriendship(string UserId, string friendId)
        {
            string relationshipId = await GetRelationshipId(UserId, Relation.None, friendId);
            return await Delete(relationshipId);
        }

        public async Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string CurrentUserId, List<UserModel> users)
        {
            List<UserRelationshipsWithOneUserDto> relationships = new List<UserRelationshipsWithOneUserDto>();

            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                users.RemoveAll(u => u.UserId == CurrentUserId); //remove Searcher from list
            }

            foreach (var user in users)
            {
                Relation relation = await GetRelationshipsBetweenTwoUsers(CurrentUserId, user.UserId);
                var s = new UserRelationshipsWithOneUserDto(user.UserId, user.Name, relation);
                if (s != null)
                {
                    relationships.Add(s);
                }

                relation = Relation.None;
            }

            return relationships;
        }

        public async Task<Dictionary<string, string>> FriendRequestsToUser(string userId)
        {
            List<RelationshipModel> relationships = await _context.Relationships.Find<RelationshipModel>(r => r.UserId2 == userId && r.Reletion == Relation.FriendRequestTo).ToListAsync(); ;

            Dictionary<string, string> friendRequests = new Dictionary<string, string>();

            foreach (var relation in relationships)
            {
                var users = await _userRepo.GetByCondation(u => u.UserId == relation.UserId2);
                friendRequests.Add(relation.UserId1, users.First().Name);
            }
            return friendRequests;
        }

        public async Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression)
        {
            return await _context.Relationships.Find(expression).ToListAsync();
        }
    }
}