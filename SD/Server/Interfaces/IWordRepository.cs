using SD.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sd.Api.Interfaces
{
    public interface IWordRepository
    {
        Task<List<WordModel>> GetAllWords(string CurrentUserId, string userId);
        Task<WordModel> GetWordById(string id);
        Task<WordModel> GetWordByText(string userId, string text);
        Task<bool> AddWord(WordModel word);
        Task<bool> UpdateWord(string id, WordModel newWord);
        Task<bool> RemoveWord(string id);
    }
}
