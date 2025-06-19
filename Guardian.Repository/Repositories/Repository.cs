using Guardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Guardian.Repository.Extensions;
using Guardian.Repository.Helpers;
using Guardian.Repository.Interfaces;
using Guardian.Repository.Migrations;
using System.Linq.Expressions;

namespace Guardian.Repository.Repositories
{
    public class Repository<TEntity>(MonitoringContext context) : IRepository<TEntity>, IDisposable where TEntity : class
    {
        private readonly MonitoringContext _context = context;

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsTracking();

            if (includes != null)
            {
                query = includes(query);
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Tuple<IQueryable<TEntity>, int>> SearchAsync(Expression<Func<TEntity, bool>> predicate, int pageNo, int pageSize, string orderBy, SortOrder sorting, CancellationToken cancellationToken, Includes<TEntity>? includes = null)
        {
            var skip = (pageNo - 1) * pageSize;

            IQueryable<TEntity> query = _context.Set<TEntity>();

            orderBy = orderBy.UpperCaseFirstLetter();

            if (includes != null)
            {
                query = includes(query);
            }

            var items = query.OrderBy(orderBy, sorting == SortOrder.DESCENDING).Where(predicate);
            var count = await items.CountAsync(cancellationToken).ConfigureAwait(false);

            return Tuple.Create(items.Skip(skip).Take(pageSize), count);
        }

        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _context.Entry(entity).Reload();

            return entity;
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (includes != null)
            {
                query = includes(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, Includes<TEntity>? includes = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null)
            {
                query = includes(query);
            }

            return await query.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context?.Dispose();
        }
    }
}