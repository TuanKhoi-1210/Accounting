namespace Accounting.Application.DTOs;

public sealed record GrnDraftDto(
    long PoId,
    long SupplierId,
    string SupplierName,
    string SuggestedPnCode,
    DateTime Now,
    List<GrnDraftLine> Lines
);

public sealed record GrnDraftLine(
    long ItemId,
    string ItemCode,
    string ItemName,
    string Uom,
    decimal OrderedQty,
    decimal ReceivedQty,
    decimal RemainingQty,
    decimal Qty,          // mặc định = RemainingQty (cho UI)
    decimal UnitCost,     // lấy từ PO line
    decimal TaxRate       // lấy từ PO line
);
