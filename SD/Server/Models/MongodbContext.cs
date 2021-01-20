using MongoDB.Driver;
using SD.Shared;
using System;

namespace sd.Api.Models
{
    public class MongodbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongodbSettings _settings;

        public MongodbContext(IMongodbSettings settings)
        {
            _settings = settings;
            try
            {
                var client = new MongoClient(_settings.ConnectionString);
                if (client != null)
                    _database = client.GetDatabase(_settings.DatabaseName);
            }
            catch
            {}
        }

        public IMongoCollection<UserModel> Users =>
            _database.GetCollection<UserModel>(_settings.UserCollectionName);
        public IMongoCollection<WordModel> Words =>
            _database.GetCollection<WordModel>(_settings.WordCollectionName);
        public IMongoCollection<OtherPageModel> OtherPages =>
            _database.GetCollection<OtherPageModel>(_settings.OtherPageCollectionName);
    }
}