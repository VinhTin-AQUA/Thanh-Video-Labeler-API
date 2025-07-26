using ExcelVideoLabler.Domain.Entities;
using ExcelVideoLabler.Infrastructure.DataContext;
using ExcelVideoLabler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabler.Infrastructure.Repositories.ConfigRepository
{
    public interface IConfigCommandRepository : ICommandRepository<Config>
    {
        Task AddByOther();
    }
    
    public class ConfigCommandRepository(AppDbContext context)
        : CommandRepository<Config>(context), IConfigCommandRepository
    {
        public Task AddByOther()
        {
            throw new NotImplementedException();
        }
    }
}