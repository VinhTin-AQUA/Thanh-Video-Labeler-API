using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Domain.Entities.Common;
using ExcelVideoLabeler.Infrastructure.Repositories.Models;

namespace ExcelVideoLabeler.Infrastructure.Repositories.Common
{
    public interface IQueryRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(int id);
        
        Task<IEnumerable<T>> GetAllAsync();
        
        Task<List<T>> FilterAsync(QueryOptions<T, T> options);
        
        Task<bool> ExistsAsync();
    }
}