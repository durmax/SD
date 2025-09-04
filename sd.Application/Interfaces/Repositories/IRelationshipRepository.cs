using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Interfaces.Repositories;
public interface IRelationshipRepository
{
    Task<RelationshipModel> GetById(string id);
    Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression);
    Task<bool> Create(RelationshipModel entity);
    Task<bool> Update(RelationshipModel entity);
    Task<bool> Delete(string id);
}
