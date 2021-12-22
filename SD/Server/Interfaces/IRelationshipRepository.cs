using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;

namespace sd.Api.Interfaces
{
    public interface IRelationshipRepository
    {
        Task<RelationshipModel> GetRelationshipById(string id);

        Task<string> GetRelationshipId(string UserId1, Relation reletion, string UserId2);

        Task<List<RelationshipModel>> GetAllFrindsRelationships(string userId);
        // Task<List<Relationship>> FrindRequestsFromUser(string userId);
        Task<List<RelationshipModel>> FriendRequestsToUser(string userId);
        Task<Relation> GetRelationshipsBetweenTwoUsers(string userId1, string userId2);

        Task<bool> Create(RelationshipModel relationship);
        Task<bool> Updat(string oldRelationshipId, RelationshipModel newRelationship);
        Task<bool> Delete(string oldRelationshipId);
    }
}
