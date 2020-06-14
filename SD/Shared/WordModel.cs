using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
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
        public string Type { get; set; }
        public string Explain { get; set; }
        public string Meanings { get; set; }

        [StringLength(20)]
        public string Box { get; set; }
        public string Level { get; set; }

        [StringLength(20)]
        public string Category { get; set; }
    }

}
