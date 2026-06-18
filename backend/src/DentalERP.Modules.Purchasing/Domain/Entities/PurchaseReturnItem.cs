namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseReturnItem
{
    public Guid Id { get; private set; }
    public Guid ReturnId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid? BatchId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }

    private PurchaseReturnItem() { }

    public static PurchaseReturnItem Create(Guid returnId, Guid itemId, decimal quantity,
        decimal unitCost, Guid? batchId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (unitCost < 0)  throw new ArgumentException("Unit cost cannot be negative.");

        return new PurchaseReturnItem
        {
            Id = Guid.NewGuid(),
            ReturnId = returnId,
            ItemId = itemId,
            BatchId = batchId,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost
        };
    }
}
