using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SD.Shared
{
    [Serializable]
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }
        public bool IsEmailReg { get; set; }
        public string EmailRegCode { get; set; }
        [BsonIgnoreIfNull]
        public int AccountLavel { get; set; }
    }
}
