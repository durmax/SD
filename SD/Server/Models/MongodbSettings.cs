namespace sd.Api.Models
{
    public class MongodbSettings : IMongodbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string WordCollectionName { get; set; }
        public string OtherPageCollectionName { get; set; }
    }

    public interface IMongodbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string WordCollectionName { get; set; }
        public string OtherPageCollectionName { get; set; }
    }
}
