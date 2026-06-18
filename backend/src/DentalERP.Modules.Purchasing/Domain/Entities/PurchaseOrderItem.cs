namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrderItem
{
    public Guid Id { get; private set; }
    public Guid PoId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid? SupplierItemId { get; private set; }
    public decimal QuantityOrdered { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public string? Notes { get; private set; }

    private PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid poId, Guid itemId, decimal quantityOrdered,
        decimal unitCost, Guid? supplierItemId = null, string? notes = null)
    {
        if (quantityOrdered <= 0) throw new ArgumentException("Quantity ordered must be greater than zero.");
        if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");

        return new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PoId = poId,
            ItemId = itemId,
            SupplierItemId = supplierItemId,
            QuantityOrdered = quantityOrdered,
            QuantityReceived = 0,
            UnitCost = unitCost,
            TotalCost = quantityOrdered * unitCost,
            Notes = notes
        };
    }

    public void AddReceived(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Received quantity must be greater than zero.");
        QuantityReceived += quantity;
    }
}
