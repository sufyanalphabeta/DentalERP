using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Assets.Domain.Entities;

public sealed class Asset : BaseEntity
{
    public static readonly string[] ValidStatuses = ["Active", "UnderMaintenance", "Disposed"];

    public string AssetTag { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public DateOnly? PurchaseDate { get; private set; }
    public decimal? PurchaseCost { get; private set; }
    public string? Location { get; private set; }
    public string Status { get; private set; } = "Active";
    public string? SerialNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedById { get; private set; }

    private Asset() { }

    public static Asset Create(string assetTag, string name, Guid? categoryId,
        DateOnly? purchaseDate, decimal? purchaseCost, string? location,
        string? serialNumber = null, string? notes = null, Guid? createdById = null)
    {
        if (string.IsNullOrWhiteSpace(assetTag)) throw new ArgumentException("Asset tag is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        return new Asset
        {
            Id = Guid.NewGuid(),
            AssetTag = assetTag.Trim().ToUpper(),
            Name = name.Trim(),
            CategoryId = categoryId,
            PurchaseDate = purchaseDate,
            PurchaseCost = purchaseCost,
            Location = location,
            Status = "Active",
            SerialNumber = serialNumber,
            Notes = notes,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, Guid? categoryId, DateOnly? purchaseDate,
        decimal? purchaseCost, string? location, string? notes)
    {
        Name = name.Trim();
        CategoryId = categoryId;
        PurchaseDate = purchaseDate;
        PurchaseCost = purchaseCost;
        Location = location;
        Notes = notes;
        Touch();
    }

    public Result SetUnderMaintenance()
    {
        if (Status == "Disposed")
            return Result.Failure(new Error("Asset.InvalidStatus", "Cannot change status of disposed asset."));
        Status = "UnderMaintenance";
        Touch();
        return Result.Success();
    }

    public Result SetActive()
    {
        if (Status == "Disposed")
            return Result.Failure(new Error("Asset.InvalidStatus", "Cannot activate disposed asset."));
        Status = "Active";
        Touch();
        return Result.Success();
    }

    public Result Dispose()
    {
        if (Status == "Disposed")
            return Result.Failure(new Error("Asset.AlreadyDisposed", "Asset is already disposed."));
        Status = "Disposed";
        SoftDelete();
        return Result.Success();
    }
}
