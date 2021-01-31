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

        public async Task<string> GetRelationshipId(string userId1, Reletion reletion, string userId2)
        {
            return await _relationshipRepository.GetRelationshipId(userId1, reletion, userId2);
        }

        public async Task<string> AreFrinds(string userId1, string userId2)
        {
            return await _relationshipRepository.GetRelationshipId(userId1, Reletion.Friend, userId2);
        }

        public async Task<bool> AddRelationship(RelationshipModel relationship)
        {
            bool res = false;
            if (await AreFrinds(relationship.UserId1, relationship.UserId2) != null)
            {
                res = await _relationshipRepository.AddRelationship(relationship);
            }
            return res;
        }

        public async Task<bool> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship)
        {
            return await _relationshipRepository.UpdatRelationship(oldRelationshipId, newRelationship);
        }

        public async Task<bool> RemoveRelationship(string relationshipId)
        {
            return await _relationshipRepository.RemoveRelationship(relationshipId);
        }

        public async Task<bool> RemoveFriendship(string UserId, string friendId)
        {
            string relationshipId = await _relationshipRepository.GetRelationshipId(UserId, Reletion.Friend, friendId);
           return await _relationshipRepository.RemoveRelationship(relationshipId);
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
                        var user = await _userService.GetUserById(relation.UserId1);
                        friendsIds.Add(relation.UserId1, user.Name);
                    }
                    if (!friendsIds.ContainsKey(relation.UserId2))
                    {
                        var user = await _userService.GetUserById(relation.UserId2);
                        friendsIds.Add(relation.UserId2, user.Name);
                    }
                }

                friendsIds.Remove(userId);
            }
            return friendsIds;
        }


        public async Task<Dictionary<string, Tuple<string, string>>> GetRelationships(string CurrentUserId, List<UserModel> users)
        {
            string relation = null;
            Dictionary<string, Tuple<string, string>> res = new Dictionary<string, Tuple<string, string>>();

            if (!string.IsNullOrWhiteSpace(CurrentUserId) && CurrentUserId != "0")
            {
                users.RemoveAll(u => u.UserId == CurrentUserId); //remove Sercher from list
            }

            foreach (var user in users)
            {

                if (CurrentUserId != null)
                {
                    var r1 = await FrindRequestsToUser(user.UserId);
                    var r2 = await FrindRequestsToUser(CurrentUserId);

                    if (r1.ContainsKey(CurrentUserId)) relation = "CrrRequest";

                    else if (r2.ContainsKey(CurrentUserId)) relation = "UserRequest";

                    else if (await AreFrinds(user.UserId, CurrentUserId) != "0")
                    {
                        relation = "Friends";
                    }

                }

                var userT = Tuple.Create(user.Name, relation);
                res.Add(user.UserId, userT);

                relation = null;
            }
            return res;
        }

        public async Task<Dictionary<string, string>> FrindRequestsToUser(string userId)
        {
            List<RelationshipModel> relationships = await _relationshipRepository.FrindRequestsToUser(userId);

            Dictionary<string, string> frindRequests = new Dictionary<string, string>();

            foreach (var relation in relationships)
            {
                var user = await _userService.GetUserById(relation.UserId2);
                frindRequests.Add(relation.UserId1, user.Name);
            }
            return frindRequests;
        }
    }
}
