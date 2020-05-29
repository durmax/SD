using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SD.Shared
{
    public class OtherPageModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string OtherPageId { get; set; }
        public string Host { get; set; }
        public string PageType { get; set; }
        public string PrimLangs { get; set; }
        public string SecLangs { get; set; }
        public int Eval { get; set; }

        public string ApiPath { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Notes { get; set; }

        public string Pattern { get; set; }
    }

}
