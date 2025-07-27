using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.ConfigRepository
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