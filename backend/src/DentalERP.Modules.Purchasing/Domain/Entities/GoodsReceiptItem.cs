namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class GoodsReceiptItem
{
    public Guid Id { get; private set; }
    public Guid GrId { get; private set; }
    public Guid? PoItemId { get; private set; }
    public Guid ItemId { get; private set; }
    public string? BatchNumber { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    private GoodsReceiptItem() { }

    public static GoodsReceiptItem Create(Guid grId, Guid itemId, decimal quantity,
        decimal unitCost, Guid? poItemId = null, string? batchNumber = null, DateOnly? expiryDate = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");

        return new GoodsReceiptItem
        {
            Id = Guid.NewGuid(),
            GrId = grId,
            PoItemId = poItemId,
            ItemId = itemId,
            BatchNumber = batchNumber,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            ExpiryDate = expiryDate
        };
    }
}
