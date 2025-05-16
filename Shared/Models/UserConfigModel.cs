using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace sd.Shared
{
    public class UserConfigModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }

        [BsonIgnoreIfNull]
        public string FirstLang { get; set; }

        [BsonIgnoreIfNull]
        public string LearnLangs { get; set; }

        [BsonIgnoreIfNull]
        public Dictionary<string, string> FavDict { get; set; }

        [BsonIgnoreIfNull]
        public string Labels { get; set; }
    }
}
