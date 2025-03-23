using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using sd.Api.Models;
using MongoDB.Driver;
using System;
using sd.Api.Infrastructure.Repositories;

namespace sd.Api.Repositories
{
    public interface IRelationshipRepository
    {
        Task<RelationshipModel> GetRelationshipById(string id);

        Task<string> GetRelationshipId(string UserId1, Relation reletion, string UserId2);

        Task<List<RelationshipModel>> GetAllFrindsRelationships(string userId);
        // Task<List<Relationship>> FrindRequestsFromUser(string userId);
        //Task<List<RelationshipModel>> FriendRequestsToUser1(string userId);
        Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2);

        Task<bool> Create(RelationshipModel relationship);
        Task<bool> Updat(string oldRelationshipId, RelationshipModel newRelationship);
        Task<bool> Delete(string oldRelationshipId);

        Task<bool> AddRelationship(RelationshipModel relationship);
        Task<bool> RemoveFriendship(string UserId, string friendId);
        Task<Dictionary<string, string>> GetAllFriends(string userId);
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
            this._userRepo = userRepo;
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

        //public async Task<List<RelationshipModel>> FriendRequestsToUser1(string userId)
        //{
        //    return await _context.Relationships.Find<RelationshipModel>(r => r.UserId2 == userId && r.Reletion == Relation.FriendRequestTo).ToListAsync();
        //}

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
        public async Task<Dictionary<string, string>> GetAllFriends(string userId)
        {
            Dictionary<string, string> friendsIds = new Dictionary<string, string>();

            List<RelationshipModel> relationships = await GetAllFrindsRelationships(userId);
            if (relationships != null)
            {
                foreach (var relation in relationships)
                {
                    if (!friendsIds.ContainsKey(relation.UserId1))
                    {
                        var user = await _userRepo.GetUserById(relation.UserId1) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId1, user.Name);
                    }
                    if (!friendsIds.ContainsKey(relation.UserId2))
                    {
                        var user = await _userRepo.GetUserById(relation.UserId2) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId2, user.Name);
                    }
                }
                friendsIds.Remove(userId);
            }
            return friendsIds;
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
                var user = await _userRepo.GetUserById(relation.UserId2);
                friendRequests.Add(relation.UserId1, user.Name);
            }
            return friendRequests;
        }
    }
}