namespace DentalERP.Modules.Laboratory.Domain.Entities;

public sealed class LabOrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public short Quantity { get; private set; } = 1;
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }

    private LabOrderItem() { }

    public static LabOrderItem Create(Guid orderId, string itemName, decimal unitCost,
        short quantity = 1, string? description = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");

        var total = unitCost * quantity;
        return new LabOrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ItemName = itemName,
            Description = description,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = total
        };
    }
}
