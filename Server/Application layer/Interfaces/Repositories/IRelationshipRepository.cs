using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application_layer.Interfaces.Repositories;
public interface IRelationshipRepository
{
    Task<RelationshipModel> GetById(string id);
    Task<IEnumerable<RelationshipModel>> GetByCondation(Expression<Func<RelationshipModel, bool>> expression);
    Task<bool> Create(RelationshipModel entity);
    Task<bool> Update(RelationshipModel entity);
    Task<bool> Delete(string id);
}
