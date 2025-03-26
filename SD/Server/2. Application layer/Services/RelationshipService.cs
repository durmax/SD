using sd.Api.Infrastructure.Repositories;
using sd.Api.Repositories;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IRelationshipService : ICrudBase<RelationshipModel>
    {
        Task<string> GetRelationshipId(string UserId1, Relation reletion, string UserId2);
        Task<bool> AddRelationship(RelationshipModel relationship);
        Task<bool> RemoveFriendship(string userId, string friendId);
        Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2);
        Task<Dictionary<string, string>> GetAllFriends(string userId);
        Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string currentUserId, List<UserModel> users);
        Task<Dictionary<string, string>> FriendRequestsToUser(string userId);
    }
    public class RelationshipService : IRelationshipService
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IUserRepository _userRepository;

        public RelationshipService(IRelationshipRepository relationshipRepository, IUserRepository userRepository)
        {
            _relationshipRepository = relationshipRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> AddRelationship(RelationshipModel relationship)
        {
            bool res = false;
            Relation relation = await _relationshipRepository.GetRelationshipsBetweenTwoUsers(relationship.UserId1, relationship.UserId2);
            if (relation == Relation.None)
            {
                res = await Create(relationship);
            }
            return res;
        }

        public async Task<bool> RemoveFriendship(string userId, string friendId)
        {
            string relationshipId = await _relationshipRepository.GetRelationshipId(userId, Relation.None, friendId);
            return await Delete(relationshipId);
        }

        public async Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2)
        {
            return await _relationshipRepository.GetRelationshipsBetweenTwoUsers(userId1, userId2);
        }

        public async Task<Dictionary<string, string>> GetAllFriends(string userId)
        {
            Dictionary<string, string> friendsIds = new Dictionary<string, string>();

            var relationships = await GetByCondation(r => (r.UserId1 == userId || r.UserId2 == userId) && r.Reletion == Relation.Friend);
            if (relationships != null)
            {
                foreach (var relation in relationships)
                {
                    if (!friendsIds.ContainsKey(relation.UserId1))
                    {
                        var users = await _userRepository.GetByCondation(u => u.UserId == relation.UserId1) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId1, users.First().Name);
                    }
                    if (!friendsIds.ContainsKey(relation.UserId2))
                    {
                        var users = await _userRepository.GetByCondation(u => u.UserId == relation.UserId2) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId2, users.First().Name);
                    }
                }
                friendsIds.Remove(userId);
            }
            return friendsIds;
        }

        public async Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string currentUserId, List<UserModel> users)
        {
            return await _relationshipRepository.GetRelationships(currentUserId, users);
        }

        public async Task<Dictionary<string, string>> FriendRequestsToUser(string userId)
        {
            return await _relationshipRepository.FriendRequestsToUser(userId);
        }

        public Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression)
        {
            return _relationshipRepository.GetByCondation(expression);
        }

        public Task<bool> Create(RelationshipModel entity)
        {
            return _relationshipRepository.Create(entity);
        }

        public Task<bool> Update(RelationshipModel entity)
        {
            return _relationshipRepository.Update(entity);
        }

        public Task<bool> Delete(string id)
        {
            return _relationshipRepository.Delete(id);
        }

        public async Task<string> GetRelationshipId(string userId1, Relation relation, string userId2)
        {
            return await _relationshipRepository.GetRelationshipId(userId1, relation, userId2);
        }
    }
}
