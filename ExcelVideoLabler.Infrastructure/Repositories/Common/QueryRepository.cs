using System.Linq.Expressions;
using ExcelVideoLabler.Domain.Entities.Common;
using ExcelVideoLabler.Infrastructure.DataContext;
using ExcelVideoLabler.Infrastructure.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabler.Infrastructure.Repositories.Common
{
    public class QueryRepository<T> : IQueryRepository<T> where T : Entity
    {
        private readonly AppDbContext context;
        private readonly DbSet<T> dbSet;

        public QueryRepository(AppDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await dbSet.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }
        
        public async Task<List<T>> FilterAsync(QueryOptions<T, T> options)
        {
            IQueryable<T> query = dbSet;

            if (options.AsNoTracking)
                query = query.AsNoTracking();

            // if (options.AsSplitQuery)
            //     query = query.AsSplitQuery();
            if (options.Include != null)
                query = options.Include(query);

            if (options.Predicate is Expression<Func<T, bool>> predicate)
                query = query.Where(predicate);

            if (options.OrderBy != null)
                query = options.OrderBy(query);

            if (options.Skip.HasValue)
                query = query.Skip(options.Skip.Value);

            if (options.Take.HasValue)
                query = query.Take(options.Take.Value);

            if (options.Selector != null)
                query = query.Select(options.Selector);

            return await query.ToListAsync(options.CancellationToken);
        }

        public async Task<bool> ExistsAsync()
        {
            var r = await dbSet.AnyAsync();
            return r;
        }
    }
}