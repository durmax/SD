using MongoDB.Driver;
using SD.Shared;

namespace sd.Api.Helpers
{
    public static class WordHelper
    {
        public static FilterDefinition<WordModel> GetFilter(string? wordId, string userId, string lang)
        {
            FilterDefinition<WordModel> filter = Builders<WordModel>.Filter.Empty;
            if (wordId != null) filter &= Builders<WordModel>.Filter.Eq(x => x.WordId, wordId);
            if (userId != null) filter &= Builders<WordModel>.Filter.Eq(x => x.UserId, userId);
            if (lang != null) filter &= Builders<WordModel>.Filter.Eq(x => x.ToLang, lang);

            return filter;
        }
    }
}
