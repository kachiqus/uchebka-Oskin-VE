using Microsoft.EntityFrameworkCore;
using oskin_site.Data;

namespace oskin_site.Services;

public sealed class RawMaterialCalculatorService
{
    private readonly ApplicationDbContext _db;

    public RawMaterialCalculatorService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> CalculateAsync(int productTypeId, int materialTypeId, decimal productCount, decimal param1, decimal param2)
    {
        if (productTypeId <= 0 || materialTypeId <= 0 || productCount <= 0 || param1 <= 0 || param2 <= 0)
            return -1;

        var productType = await _db.ProductTypes.AsNoTracking().FirstOrDefaultAsync(x => x.ProductTypeId == productTypeId);
        var materialType = await _db.MaterialTypes.AsNoTracking().FirstOrDefaultAsync(x => x.MaterialTypeId == materialTypeId);

        if (productType is null || materialType is null)
            return -1;

        var baseAmount = productCount * param1 * param2 * productType.Coefficient;
        var withLosses = baseAmount * (1 + (materialType.LossPercent / 100m));

        if (withLosses < 0)
            return -1;

        return (int)Math.Ceiling(withLosses);
    }
}
