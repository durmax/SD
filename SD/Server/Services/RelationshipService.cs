using sd.Api.Interfaces;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Services
{
    public class RelationshipService
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly UserService _userService;

        public RelationshipService(IRelationshipRepository relationshipRepository, UserService userService)
        {
            _relationshipRepository = relationshipRepository;
            _userService = userService;
        }

        public async Task<RelationshipModel> GetRelationshipById(string id)
        {
            return await _relationshipRepository.GetRelationshipById(id);
        }

        public async Task<string> GetRelationshipId(string userId1, Relation reletion, string userId2)
        {
            return await _relationshipRepository.GetRelationshipId(userId1, reletion, userId2);
        }

        public async Task<bool> AddRelationship(RelationshipModel relationship)
        {
            bool res = false;
            Relation relation = await _relationshipRepository.GetRelationshipsBetweenTwoUsers(relationship.UserId1, relationship.UserId2);
            if (relation == Relation.None)
            {
                res = await _relationshipRepository.Create(relationship);
            }
            return res;
        }

        public async Task<bool> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship)
        {
            return await _relationshipRepository.Updat(oldRelationshipId, newRelationship);
        }

        public async Task<bool> RemoveRelationship(string relationshipId)
        {
            return await _relationshipRepository.Delete(relationshipId);
        }

        public async Task<bool> RemoveFriendship(string UserId, string friendId)
        {
            string relationshipId = await _relationshipRepository.GetRelationshipId(UserId, Relation.None, friendId);
            return await _relationshipRepository.Delete(relationshipId);
        }

        public async Task<Dictionary<string, string>> GetAllFriends(string userId)
        {
            Dictionary<string, string> friendsIds = new Dictionary<string, string>();

            List<RelationshipModel> relationships = await _relationshipRepository.GetAllFrindsRelationships(userId);
            if (relationships != null)
            {
                foreach (var relation in relationships)
                {
                    if (!friendsIds.ContainsKey(relation.UserId1))
                    {
                        var user = await _userService.GetUserById(relation.UserId1) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId1, user.Name);
                    }
                    if (!friendsIds.ContainsKey(relation.UserId2))
                    {
                        var user = await _userService.GetUserById(relation.UserId2) ?? throw new NullReferenceException();
                        friendsIds.Add(relation.UserId2, user.Name);
                    }
                }
                friendsIds.Remove(userId);
            }
            return friendsIds;
        }

        public async Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string CurrentUserId, List<UserModel> users)
        {
            if (CurrentUserId == null || users == null) throw new ArgumentNullException();

            List<UserRelationshipsWithOneUserDto> relationships = new List<UserRelationshipsWithOneUserDto>();

            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                users.RemoveAll(u => u.UserId == CurrentUserId); //remove Searcher from list
            }

            foreach (var user in users)
            {
                Relation relation = await _relationshipRepository.GetRelationshipsBetweenTwoUsers(CurrentUserId, user.UserId);
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
            List<RelationshipModel> relationships = await _relationshipRepository.FriendRequestsToUser(userId);

            Dictionary<string, string> friendRequests = new Dictionary<string, string>();

            foreach (var relation in relationships)
            {
                var user = await _userService.GetUserById(relation.UserId2);
                friendRequests.Add(relation.UserId1, user.Name);
            }
            return friendRequests;
        }
    }
}
