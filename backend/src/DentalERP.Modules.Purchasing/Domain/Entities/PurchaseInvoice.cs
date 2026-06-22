using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseInvoice : BaseEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateOnly InvoiceDate { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft/Posted/Cancelled
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal NetTotal { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<PurchaseInvoiceItem> _items = [];
    public IReadOnlyList<PurchaseInvoiceItem> Items => _items;

    private PurchaseInvoice() { }

    public static PurchaseInvoice Create(string invoiceNumber, Guid supplierId,
        DateOnly invoiceDate, Guid? warehouseId = null, string? notes = null, Guid? createdById = null)
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            SupplierId = supplierId,
            InvoiceDate = invoiceDate,
            WarehouseId = warehouseId,
            Notes = notes,
            CreatedById = createdById,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

    public void AddItem(PurchaseInvoiceItem item) { _items.Add(item); RecalcTotals(); }
    public void ClearItems() { _items.Clear(); RecalcTotals(); }

    public void UpdateHeader(DateOnly invoiceDate, Guid? warehouseId, decimal discount, string? notes)
    {
        InvoiceDate = invoiceDate;
        WarehouseId = warehouseId;
        Discount = discount < 0 ? 0 : discount;
        Notes = notes;
        RecalcTotals();
        Touch();
    }

    /// <summary>
    /// Update header and set totals directly (used when items are managed via DbContext directly).
    /// </summary>
    public void UpdateDirect(DateOnly invoiceDate, Guid? warehouseId, decimal subtotal, decimal discount, string? notes)
    {
        InvoiceDate = invoiceDate;
        WarehouseId = warehouseId;
        Subtotal = subtotal;
        Discount = discount < 0 ? 0 : discount;
        NetTotal = Math.Max(0, Subtotal - Discount);
        Notes = notes;
        Touch();
    }

    public void Post() { Status = "Posted"; PostedAt = DateTime.UtcNow; Touch(); }
    public void Cancel() { Status = "Cancelled"; CancelledAt = DateTime.UtcNow; Touch(); }
    public void Delete() => SoftDelete();

    private void RecalcTotals()
    {
        Subtotal = _items.Sum(i => i.LineTotal);
        NetTotal = Subtotal - Discount;
        if (NetTotal < 0) NetTotal = 0;
    }
}
