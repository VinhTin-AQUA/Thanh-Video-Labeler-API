using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.SheetRepository
{
    public interface ISheetCommandRepository : ICommandRepository<Sheet>
    {
    }

    public class SheetCommandRepository(AppDbContext context)
        : CommandRepository<Sheet>(context), ISheetCommandRepository
    {
    }
}
