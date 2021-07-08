using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SD.Shared
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
        public string Labels { get; set; }
    }
}
