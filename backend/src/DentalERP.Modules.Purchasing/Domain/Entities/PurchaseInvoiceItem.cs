namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseInvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid ItemId { get; private set; }
    public string? ItemCode { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string? UnitName { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal? SalePrice { get; private set; }
    public decimal LineTotal { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? BatchNumber { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PurchaseInvoiceItem() { }

    public static PurchaseInvoiceItem Create(Guid invoiceId, Guid itemId, string itemName,
        decimal quantity, decimal purchasePrice, string? itemCode = null,
        string? barcode = null, string? unitName = null, decimal? salePrice = null,
        DateOnly? expiryDate = null, string? batchNumber = null, int sortOrder = 0)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be > 0");
        if (purchasePrice < 0) throw new ArgumentException("Price cannot be negative");

        return new PurchaseInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            ItemId = itemId,
            ItemCode = itemCode,
            ItemName = itemName,
            Barcode = barcode,
            UnitName = unitName,
            Quantity = quantity,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            LineTotal = quantity * purchasePrice,
            ExpiryDate = expiryDate,
            BatchNumber = batchNumber,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
}
