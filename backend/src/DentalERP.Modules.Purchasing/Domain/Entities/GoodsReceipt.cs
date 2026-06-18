using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class GoodsReceipt : BaseEntity
{
    public string GrNumber { get; private set; } = string.Empty;
    public Guid? PoId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateOnly ReceiptDate { get; private set; }
    public string? SupplierInvoiceRef { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReceivedById { get; private set; }

    private readonly List<GoodsReceiptItem> _items = [];
    public IReadOnlyList<GoodsReceiptItem> Items => _items;

    private GoodsReceipt() { }

    public static GoodsReceipt Create(string grNumber, Guid supplierId, Guid warehouseId,
        DateOnly receiptDate, Guid? poId = null, string? supplierInvoiceRef = null,
        string? notes = null, Guid? receivedById = null)
        => new()
        {
            Id = Guid.NewGuid(),
            GrNumber = grNumber,
            PoId = poId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReceiptDate = receiptDate,
            SupplierInvoiceRef = supplierInvoiceRef,
            Notes = notes,
            ReceivedById = receivedById,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

    public void AddItem(GoodsReceiptItem item)
    {
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.TotalCost);
    }
}
