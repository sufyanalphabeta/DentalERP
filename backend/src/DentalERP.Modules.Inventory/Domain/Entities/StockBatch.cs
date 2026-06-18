namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class StockBatch
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string? BatchNumber { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public DateOnly ReceivedDate { get; private set; }
    public bool IsDepleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockBatch() { }

    public static StockBatch Create(Guid itemId, Guid warehouseId, decimal quantity,
        decimal unitCost, DateOnly receivedDate, string? batchNumber = null, DateOnly? expiryDate = null)
    {
        if (quantity <= 0) throw new ArgumentException("Batch quantity must be greater than zero.");
        if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");

        return new StockBatch
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            BatchNumber = batchNumber,
            Quantity = quantity,
            UnitCost = unitCost,
            ExpiryDate = expiryDate,
            ReceivedDate = receivedDate,
            IsDepleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deduct(decimal amount)
    {
        Quantity -= amount;
        if (Quantity <= 0)
        {
            Quantity = 0;
            IsDepleted = true;
        }
    }

    public void AddQuantity(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Added quantity must be positive.");
        Quantity += amount;
        IsDepleted = false;
    }

    public bool IsExpiringSoon(int withinDays)
        => ExpiryDate.HasValue && ExpiryDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));

    public bool IsExpired
        => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
}
