using ExcelVideoLabeler.Domain.Entities.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.Common
{
    public interface ICommandRepository<T> : IDisposable where T : Entity
    {
        Task<T> AddAsync(T entity);
        
        Task<T> UpdateAsync(T entity);
        
        Task<T> DeleteAsync(T entity);
        
        Task<ICollection<T>> AddRangeAsync(ICollection<T> entities);
        
        Task<ICollection<T>> UpdateRangeAsync(ICollection<T> entities);
    }
}