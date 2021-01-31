using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SD.Shared
{
    public class RelationshipModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string RelationshipId { get; set; }
        [Required]
        public string UserId1 { get; set; }

        [Required]
        public Reletion Reletion { get; set; }

        [Required]
        public string UserId2 { get; set; }
    }

    public enum Reletion
    {
        None,
        Block, 
        Follow,
        FriendRequest,
        Friend
    }

}
