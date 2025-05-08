namespace sd.Api.Models
{
    public class MongodbSettings : IMongodbSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string UserCollectionName { get; set; }
        public required string WordCollectionName { get; set; }
        public required string OtherPageCollectionName { get; set; }
        public required string RelationshipCollectionName { get; set; }
        public required string UsersConfigsCollectionName { get; set; }
    }

    public interface IMongodbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string WordCollectionName { get; set; }
        public string OtherPageCollectionName { get; set; }
        public string RelationshipCollectionName { get; set; }
        public string UsersConfigsCollectionName { get; set; }
    }
}
