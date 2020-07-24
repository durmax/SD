using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SD.Shared
{
    public class WordModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string WordId { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(100)]
        public String Title { get; set; }

        [StringLength(10)]
        public string WordLang { get; set; }
        [StringLength(10)]
        public string ToLang { get; set; }
        [BsonIgnoreIfNull]
        [StringLength(10)]
        public string Type { get; set; }
        [BsonIgnoreIfNull]
        public string Explain { get; set; }
        [BsonIgnoreIfNull]
        public string Meanings { get; set; }
        [StringLength(20)]
        public string Box { get; set; }
        [BsonIgnoreIfNull]
        public string Level { get; set; }
        [BsonIgnoreIfNull]
        [StringLength(20)]
        public string Category { get; set; }

        [BsonIgnoreIfNull]      
        public int ShareWith { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Likes { get; set; }
    }

}
