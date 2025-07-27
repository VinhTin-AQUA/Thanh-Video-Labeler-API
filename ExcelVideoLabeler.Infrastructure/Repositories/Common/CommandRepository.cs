using ExcelVideoLabeler.Domain.Entities.Common;
using ExcelVideoLabeler.Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;

namespace ExcelVideoLabeler.Infrastructure.Repositories.Common
{
    public class CommandRepository<T> : ICommandRepository<T>, IAsyncDisposable where T : Entity
    {
        protected readonly AppDbContext context;
        protected readonly DbSet<T> dbSet;

        public CommandRepository(AppDbContext context)
        {
            this.context = context;
            dbSet = this.context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<T>> AddRangeAsync(ICollection<T> entities)
        {
            dbSet.AddRange(entities!);  
            await context.SaveChangesAsync();
            return entities!;
        }

        public async Task<ICollection<T>> UpdateRangeAsync(ICollection<T> entities)
        {
            dbSet.UpdateRange(entities);
            await context.SaveChangesAsync();
            return entities;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
        }
    }
}