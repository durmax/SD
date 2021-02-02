using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;

namespace sd.Api.Interfaces
{
    public interface IRelationshipRepository
    {
        Task<RelationshipModel> GetRelationshipById(string id);

        Task<string> GetRelationshipId(string UserId1, Relation reletion, string UserId2);

        Task<bool> AddRelationship(RelationshipModel relationship);
        Task<bool> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship);
        Task<bool> RemoveRelationship(string oldRelationshipId);

        Task<List<RelationshipModel>> GetAllFrindsRelationships(string userId);
        // Task<List<Relationship>> FrindRequestsFromUser(string userId);
        Task<List<RelationshipModel>> FriendRequestsToUser(string userId);
        Task<List<Relation>> GetRelationshipsBetweenTwoUsers(string userId1, string userId2);
    }
}
