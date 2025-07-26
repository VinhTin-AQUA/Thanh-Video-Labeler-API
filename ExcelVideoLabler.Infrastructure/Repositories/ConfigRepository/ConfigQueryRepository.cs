using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Infrastructure.DataContext;
using ExcelVideoLabler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabler.Infrastructure.Repositories.ConfigRepository
{
    public interface IConfigQueryRepository : IQueryRepository<Config>
    {
        Task GetAllByOther();
    }
    
    public class ConfigQueryRepository(AppDbContext context)
        : QueryRepository<Config>(context), IConfigQueryRepository
    {
        public Task GetAllByOther()
        {
            throw new NotImplementedException();
        }
    }
}