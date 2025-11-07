using Accounting.Application.DTOs;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Accounting.Application.Services;

public class GrnService
{
    private readonly IDbContextFactory<AccountingDbContext> _dbFactory;
    public GrnService(IDbContextFactory<AccountingDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<GrnDraftDto> GetGrnDraftFromPoAsync(long poId, CancellationToken ct = default)
    {
        await using var db = _dbFactory.CreateDbContext();

        var po = await db.DonMua
            .Where(p => p.Id == poId)
            .Select(p => new
            {
                p.Id,
                p.NhaCungCapId,
                SupplierName = db.NhaCungCap.Where(n => n.Id == p.NhaCungCapId).Select(n => n.Ten).FirstOrDefault(),
                Lines = db.DonMuaDong.Where(l => l.DonMuaId == p.Id).Select(l => new
                {
                    l.VatTuId,
                    ItemCode = db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ma).FirstOrDefault(),
                    ItemName = db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.Ten).FirstOrDefault(),
                    Uom = db.DonViTinh.Where(d => d.Id == db.VatTu.Where(v => v.Id == l.VatTuId).Select(v => v.DonViTinhId).FirstOrDefault()).Select(d => d.Ten).FirstOrDefault(),
                    OrderedQty = l.SoLuong,
                    UnitPrice = l.DonGia,
                    TaxRate = db.ThueSuat.Where(t => t.Id == l.ThueSuatId).Select(t => t.TyLe).FirstOrDefault()
                }).ToList()
            })
            .FirstAsync(ct);

        var receivedByItem = await db.PhieuNhap
             .Where(pn => pn.DonMuaId == po.Id)
             .SelectMany(pn => pn.Dong)
             .Where(d => d.VatTuId != null)          // FIX
             .GroupBy(d => d.VatTuId!.Value)         // FIX: key = long (not nullable)
             .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.SoLuong) })
             .ToDictionaryAsync(x => x.ItemId, x => x.Qty, ct);



        var lines = po.Lines
            .Where(l => l.VatTuId != null) // FIX
            .GroupBy(l => new {
                VatTuId = l.VatTuId!.Value,   // FIX
                l.ItemCode,
                l.ItemName,
                l.Uom,
                l.UnitPrice,
                l.TaxRate
            })
            .Select(g =>
            {
                var ordered = g.Sum(x => x.OrderedQty);
                decimal received = 0m;
                receivedByItem.TryGetValue(g.Key.VatTuId, out received); // dùng g.Key
                var remaining = ordered - received;

                return new GrnDraftLine(
                    ItemId: g.Key.VatTuId,
                    ItemCode: g.Key.ItemCode ?? "",
                    ItemName: g.Key.ItemName ?? "",
                    Uom: g.Key.Uom ?? "",
                    OrderedQty: ordered,
                    ReceivedQty: received,
                    RemainingQty: remaining,
                    Qty: remaining > 0 ? remaining : 0m,
                    UnitCost: g.Key.UnitPrice,
                    TaxRate: g.Key.TaxRate
                );
            })
            .Where(x => x.RemainingQty > 0)
            .OrderBy(x => x.ItemName)
            .ToList();



        return new GrnDraftDto(
            PoId: po.Id,
            SupplierId: po.NhaCungCapId,
            SupplierName: po.SupplierName ?? "",
            SuggestedPnCode: await GenerateNextPnCodeAsync(db, ct),
            Now: DateTime.Now,
            Lines: lines
        );
    }

    private static async Task<string> GenerateNextPnCodeAsync(AccountingDbContext db, CancellationToken ct)
    {
        var ymd = DateTime.Now.ToString("yyyyMMdd");
        var count = await db.PhieuNhap.CountAsync(ct);
        return $"PN-{ymd}-{(count + 1):D4}";
    }
}
