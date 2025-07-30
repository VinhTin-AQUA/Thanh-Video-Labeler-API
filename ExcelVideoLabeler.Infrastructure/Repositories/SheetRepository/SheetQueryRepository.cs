using ExcelVideoLabeler.Domain.Entities;
using ExcelVideoLabeler.Infrastructure.DataContext;
using ExcelVideoLabeler.Infrastructure.Repositories.Common;

namespace ExcelVideoLabeler.Infrastructure.Repositories.SheetRepository
{
    public interface ISheetQueryRepository : IQueryRepository<Sheet>
    {
    }

    public class SheetQueryRepository(AppDbContext context)
        : QueryRepository<Sheet>(context), ISheetQueryRepository
    {
    }
}
