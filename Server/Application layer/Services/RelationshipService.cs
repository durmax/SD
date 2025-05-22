using sd.Api.Infrastructure.Repositories;
using sd.Api.Repositories;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application.Services
{
    public interface IRelationshipService
    {
        Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression);
        Task<bool> Create(RelationshipModel entity);
        Task<bool> Update(RelationshipModel entity);
        Task<bool> Delete(string id);

        Task<RelationshipModel> GetRelationship(string UserId1, Relation reletion, string UserId2);
        Task<bool> AddRelationship(RelationshipModel relationship);
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
            var rs = await _relationshipRepository.GetByCondation(x =>
                (x.UserId1 == relationship.UserId1 && x.UserId2 == relationship.UserId2) ||
                 (x.UserId1 == relationship.UserId2 && x.UserId2 == relationship.UserId1)
                 );

            bool res = false;

            if (rs?.FirstOrDefault()?.Reletion == Relation.None)
            {
                res = await Create(relationship);
            }
            return res;
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
            List<UserRelationshipsWithOneUserDto> relationships = new List<UserRelationshipsWithOneUserDto>();

            if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId != "0")
            {
                users.RemoveAll(u => u.UserId == currentUserId); //remove Searcher from list
            }

            foreach (var user in users)
            {
                var r = await GetRelationship(currentUserId, Relation.None, user.UserId);
                var s = new UserRelationshipsWithOneUserDto(user.UserId, user.Name, r.Reletion);
                if (s != null)
                {
                    relationships.Add(s);
                }

                r.Reletion = Relation.None;
            }

            return relationships;
        }

        public async Task<Dictionary<string, string>> FriendRequestsToUser(string userId)
        {
            var relationships = await GetByCondation(r => r.UserId2 == userId && r.Reletion == Relation.FriendRequestTo);

            Dictionary<string, string> friendRequests = new Dictionary<string, string>();

            foreach (var relation in relationships)
            {
                var users = await _userRepository.GetByCondation(u => u.UserId == relation.UserId2);
                friendRequests.Add(relation.UserId1, users.First().Name);
            }
            return friendRequests;
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

        public async Task<RelationshipModel> GetRelationship(string userId1, Relation relation, string userId2)
        {
            var rs = await _relationshipRepository.GetByCondation(x =>
                (relation == Relation.None || x.Reletion == relation) &&
                ((x.UserId1 == userId1 && x.UserId2 == userId2) ||
                 (x.UserId1 == userId2 && x.UserId2 == userId1))
                );

            return rs?.FirstOrDefault();
        }
    }
}
