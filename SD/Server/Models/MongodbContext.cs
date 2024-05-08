using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SD.Shared;
using System;

namespace sd.Api.Models
{
    public class MongodbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongodbSettings _settings;
        private readonly ILogger<MongodbContext> _logger;

        public MongodbContext(IMongodbSettings settings, ILogger<MongodbContext> logger)
        {
            _settings = settings;
            _logger = logger;
            try
            {
                var client = new MongoClient(_settings.ConnectionString);
                if (client != null)
                    _database = client.GetDatabase(_settings.DatabaseName);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.ToString());
            }
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