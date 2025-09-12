using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Interfaces.Repositories;
public interface IWordRepository
{
    Task<IEnumerable<WordModel>> GetByCondation(Expression<Func<WordModel, bool>> expression);
    Task<WordModel> GetById(string id);
    Task<bool> Create(WordModel entity);
    Task<bool> Update(WordModel entity);
    Task<bool> Delete(string id);

    Task<long> GetDocCount(string userId, string lang);
    Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit);
    Task<List<WordModel>> GetWords(string userId, string lang, int skip, int limit);

}
