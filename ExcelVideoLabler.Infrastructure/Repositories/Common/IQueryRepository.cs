using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Domain.Entities.Common;
using ExcelVideoLabler.Infrastructure.Repositories.Models;

namespace ExcelVideoLabler.Infrastructure.Repositories.Common
{
    public interface IQueryRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(int id);
        
        Task<IEnumerable<T>> GetAllAsync();
        
        Task<List<T>> FilterAsync(QueryOptions<T, T> options);
        
        Task<bool> ExistsAsync();
    }
}