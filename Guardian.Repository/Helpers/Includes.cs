using Microsoft.EntityFrameworkCore.Query;

namespace Guardian.Repository.Helpers
{
    public delegate IIncludableQueryable<TEntity, object> Includes<TEntity>(IQueryable<TEntity> query);
}