using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SD.Shared;
using System;

namespace sd.Api.Models
{
    public class MongodbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IOptions<MongodbSettings> _settings;

        public MongodbContext(IOptions<MongodbSettings> settings)
        {
            _settings = settings;
            try
            {
                var client = new MongoClient(_settings.Value.ConnectionString);
                if (client != null)
                    _database = client.GetDatabase(_settings.Value.DatabaseName);
            }
            catch(Exception ex)
            {

            }

        }

        public IMongoCollection<UserModel> Users =>
            _database.GetCollection<UserModel>(_settings.Value.UserCollectionName);
        public IMongoCollection<WordModel> Words =>
            _database.GetCollection<WordModel>(_settings.Value.WordCollectionName);
        public IMongoCollection<OtherPageModel> OtherPages =>
            _database.GetCollection<OtherPageModel>(_settings.Value.OtherPageCollectionName);
    }
}