using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class CommentModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string CommentId { get; set; }
        public string UserId { get; set; }
        public string CommentOwnerName { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [BsonIgnoreIfNull]
        public DateTime UpdatedAt { get; set; }
        [BsonIgnoreIfNull]
        public String CommentText { get; set; }
        [BsonIgnoreIfNull]
        public List<string> Likes { get; set; }

        [BsonIgnoreIfNull]
        public List<CommentModel> Comments { get; set; }
    }
}
