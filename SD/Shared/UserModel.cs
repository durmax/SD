using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsEmailReg { get; set; }
        public string EmailRegCode { get; set; }
        [BsonIgnoreIfNull]
        public int AccountLavel { get; set; }
        //[BsonIgnoreIfNull]
        //public string FirstLang { get; set; }
        //[BsonIgnoreIfNull]
        //public string LearnLangs { get; set; }
        //[BsonIgnoreIfNull]
        //public string Role { get; set; }
    }
}
