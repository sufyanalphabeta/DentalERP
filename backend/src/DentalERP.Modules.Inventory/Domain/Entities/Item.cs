using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class Item : BaseEntity
{
    public string ItemCode { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? UnitOfMeasureId { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal ReorderLevel { get; private set; }
    public decimal ReorderQuantity { get; private set; }
    public bool IsExpiryTracked { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public string? StorageConditions { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private readonly List<ItemBarcode> _barcodes = [];
    public IReadOnlyList<ItemBarcode> Barcodes => _barcodes;

    private Item() { }

    public static Item Create(
        string itemCode,
        string name,
        string? nameAr = null,
        string? barcode = null,
        Guid? categoryId = null,
        Guid? unitOfMeasureId = null,
        decimal unitCost = 0,
        decimal reorderLevel = 0,
        decimal reorderQuantity = 0,
        bool isExpiryTracked = false,
        bool allowNegativeStock = false,
        string? storageConditions = null,
        string? notes = null)
    {
        if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");
        if (reorderLevel < 0) throw new ArgumentException("Reorder level cannot be negative.");

        return new Item
        {
            Id = Guid.NewGuid(),
            ItemCode = itemCode,
            Barcode = barcode,
            Name = name,
            NameAr = nameAr,
            CategoryId = categoryId,
            UnitOfMeasureId = unitOfMeasureId,
            UnitCost = unitCost,
            ReorderLevel = reorderLevel,
            ReorderQuantity = reorderQuantity,
            IsExpiryTracked = isExpiryTracked,
            AllowNegativeStock = allowNegativeStock,
            StorageConditions = storageConditions,
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, Guid? categoryId, Guid? unitOfMeasureId,
        decimal reorderLevel, decimal reorderQuantity, bool isExpiryTracked,
        bool allowNegativeStock, string? storageConditions, string? notes)
    {
        Name = name;
        NameAr = nameAr;
        CategoryId = categoryId;
        UnitOfMeasureId = unitOfMeasureId;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        IsExpiryTracked = isExpiryTracked;
        AllowNegativeStock = allowNegativeStock;
        StorageConditions = storageConditions;
        Notes = notes;
        Touch();
    }

    public void UpdateCost(decimal newCost)
    {
        if (newCost < 0) throw new ArgumentException("Unit cost cannot be negative.");
        UnitCost = newCost;
        Touch();
    }

    public Result AddBarcode(ItemBarcode barcode)
    {
        if (_barcodes.Any(b => b.Barcode == barcode.Barcode))
            return Result.Failure(new Error("Item.DuplicateBarcode", "Barcode already exists on this item."));
        _barcodes.Add(barcode);
        return Result.Success();
    }

    public void RemoveBarcode(Guid barcodeId)
        => _barcodes.RemoveAll(b => b.Id == barcodeId);

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }
    public void Delete()     => SoftDelete();
}
