namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class SupplierItem
{
    public Guid Id { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid ItemId { get; private set; }
    public string SupplierItemCode { get; private set; } = string.Empty;
    public string? SupplierItemName { get; private set; }
    public decimal? LastUnitCost { get; private set; }
    public bool IsPreferred { get; private set; }
    public string? Notes { get; private set; }

    private SupplierItem() { }

    public static SupplierItem Create(Guid supplierId, Guid itemId, string supplierItemCode,
        string? supplierItemName = null, decimal? lastUnitCost = null, bool isPreferred = false, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(supplierItemCode))
            throw new ArgumentException("Supplier item code is required.");

        return new SupplierItem
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            ItemId = itemId,
            SupplierItemCode = supplierItemCode.Trim(),
            SupplierItemName = supplierItemName,
            LastUnitCost = lastUnitCost,
            IsPreferred = isPreferred,
            Notes = notes
        };
    }

    public void UpdateCost(decimal newCost) => LastUnitCost = newCost;
    public void SetPreferred(bool value) => IsPreferred = value;
}
