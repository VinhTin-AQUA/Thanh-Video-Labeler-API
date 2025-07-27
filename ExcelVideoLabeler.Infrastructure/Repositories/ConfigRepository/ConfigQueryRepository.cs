using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository
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