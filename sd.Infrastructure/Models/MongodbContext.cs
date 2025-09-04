using MongoDB.Driver;
using sd.Shared;

namespace sd.Api.Infrastructure.Models
{
    public class MongodbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongodbSettings _settings;

        public MongodbContext(IMongodbSettings settings)
        {
            _settings = settings;

                var client = new MongoClient(_settings.ConnectionString);
                if (client != null)
                    _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<UserModel> Users =>
            _database.GetCollection<UserModel>(_settings.UserCollectionName);
        public IMongoCollection<RelationshipModel> Relationships =>
             _database.GetCollection<RelationshipModel>(_settings.RelationshipCollectionName);
        public IMongoCollection<WordModel> Words =>
            _database.GetCollection<WordModel>(_settings.WordCollectionName);
        public IMongoCollection<OtherPageModel> OtherPages =>
            _database.GetCollection<OtherPageModel>(_settings.OtherPageCollectionName);
        public IMongoCollection<UserConfigModel> UsersConfigs =>
            _database.GetCollection<UserConfigModel>(_settings.UsersConfigsCollectionName);
    }
}