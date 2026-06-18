namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class ItemBarcode
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public string Barcode { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public bool IsPrimary { get; private set; }

    private ItemBarcode() { }

    public static ItemBarcode Create(Guid itemId, string barcode, string? label = null, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode value is required.");
        return new ItemBarcode
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            Barcode = barcode.Trim(),
            Label = label,
            IsPrimary = isPrimary
        };
    }
}
