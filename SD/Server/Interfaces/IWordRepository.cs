using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IWordRepository
    {
        Task<long> GetDocCount(string userId, string lang);
        Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit);
        Task<WordModel> GetWordById(string id);
        Task<WordModel> GetWordByText(string userId, string text);
        Task<IEnumerable<WordModel>> GetWordsContainText(string userId, string text);

        Task<bool> Create(WordModel word);
        Task<bool> Update(string id, WordModel newWord);
        Task<bool> Delete(string id);
    }
}
