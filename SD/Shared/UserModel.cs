using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SD.Shared
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string EmailRegCode { get; set; }
        public bool IsEmailReg { get; set; }
        public int AccountLavel { get; set; }

        public string FirstLang { get; set; }
        public List<string> KnownLangs { get; set; }
        public List<string> LearnLangs { get; set; }

        public List<string> FriendRequests { get; set; }
        public List<string> Friends { get; set; }

        public string Role { get; set; }
    }
}
