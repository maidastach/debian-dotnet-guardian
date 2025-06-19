using Guardian.Domain.Entities;
using Guardian.Repository.Helpers;
using System.Linq.Expressions;

namespace Guardian.Repository.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null);
        Task<Tuple<IQueryable<TEntity>, int>> SearchAsync(Expression<Func<TEntity, bool>> predicate, int pageNo, int pageSize, string orderBy, SortOrder sorting, CancellationToken cancellationToken, Includes<TEntity>? includes = null);
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);
        void Delete(TEntity entity);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null);
    }
}