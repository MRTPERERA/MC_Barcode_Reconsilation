using MC_Barcode_Reconciliation_API.Data;
using MC_Barcode_Reconciliation_API.DTOs;
using MC_Barcode_Reconciliation_API.Models;
using Microsoft.EntityFrameworkCore;

namespace MC_Barcode_Reconciliation_API.Services;

public class ScanBrcodePrintService : IScanBrcodePrintService
{
    private readonly AppDbContext _context;

    public ScanBrcodePrintService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ScanBrcodePrintDto>> GetAllAsync(ScanBrcodePrintFilterDto filter)
    {
        var query = _context.ScanBrcodePrints.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Site))
            query = query.Where(x => x.SITE == filter.Site);

        if (!string.IsNullOrWhiteSpace(filter.PartNo))
            query = query.Where(x => x.PartNo == filter.PartNo);

        if (!string.IsNullOrWhiteSpace(filter.Machine))
            query = query.Where(x => x.Machine == filter.Machine);

        if (!string.IsNullOrWhiteSpace(filter.ShopOrder))
            query = query.Where(x => x.ShopOrder == filter.ShopOrder);

        if (!string.IsNullOrWhiteSpace(filter.LoadingId))
            query = query.Where(x => x.LoadingId == filter.LoadingId);

        if (filter.ProdDateFrom.HasValue)
            query = query.Where(x => x.ProdDate >= filter.ProdDateFrom.Value);

        if (filter.ProdDateTo.HasValue)
            query = query.Where(x => x.ProdDate <= filter.ProdDateTo.Value);

        var skip = (filter.Page - 1) * filter.PageSize;

        return await query
            .OrderByDescending(x => x.ProdDate)
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<ScanBrcodePrintDto?> GetByRandomCodeAsync(string randomCode)
    {
        var entity = await _context.ScanBrcodePrints
            .FirstOrDefaultAsync(x => x.RandomCode == randomCode);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<ScanBrcodePrintDto?> UpdateAsync(string randomCode, UpdateScanBrcodePrintDto dto)
    {
        var entity = await _context.ScanBrcodePrints
            .FirstOrDefaultAsync(x => x.RandomCode == randomCode);

        if (entity is null)
            return null;

        entity.SerialNo = dto.SerialNo ?? entity.SerialNo;
        entity.Barcode = dto.Barcode ?? entity.Barcode;
        entity.SITE = dto.SITE ?? entity.SITE;
        entity.ProdDate = dto.ProdDate ?? entity.ProdDate;
        entity.ProdShift = dto.ProdShift ?? entity.ProdShift;
        entity.PartNo = dto.PartNo ?? entity.PartNo;
        entity.Description = dto.Description ?? entity.Description;
        entity.Machine = dto.Machine ?? entity.Machine;
        entity.ShopOrder = dto.ShopOrder ?? entity.ShopOrder;
        entity.DopId = dto.DopId ?? entity.DopId;
        entity.EpfNo = dto.EpfNo ?? entity.EpfNo;
        entity.PrintedQty = dto.PrintedQty ?? entity.PrintedQty;
        entity.PrintedDate = dto.PrintedDate ?? entity.PrintedDate;
        entity.ScnQty = dto.ScnQty ?? entity.ScnQty;
        entity.ScanDate = dto.ScanDate ?? entity.ScanDate;
        entity.LoadingId = dto.LoadingId ?? entity.LoadingId;

        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> ExistsAsync(string randomCode)
    {
        return await _context.ScanBrcodePrints.AnyAsync(x => x.RandomCode == randomCode);
    }

    private static ScanBrcodePrintDto MapToDto(ScanBrcodePrint entity) => new()
    {
        RandomCode = entity.RandomCode,
        SerialNo = entity.SerialNo,
        Barcode = entity.Barcode,
        SITE = entity.SITE,
        ProdDate = entity.ProdDate,
        ProdShift = entity.ProdShift,
        PartNo = entity.PartNo,
        Description = entity.Description,
        Machine = entity.Machine,
        ShopOrder = entity.ShopOrder,
        DopId = entity.DopId,
        EpfNo = entity.EpfNo,
        PrintedQty = entity.PrintedQty,
        PrintedDate = entity.PrintedDate,
        ScnQty = entity.ScnQty,
        ScanDate = entity.ScanDate,
        LoadingId = entity.LoadingId
    };
}
