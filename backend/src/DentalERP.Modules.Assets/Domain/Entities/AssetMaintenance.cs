using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Assets.Domain.Entities;

public sealed class AssetMaintenance : BaseEntity
{
    public Guid AssetId { get; private set; }
    public DateOnly MaintenanceDate { get; private set; }
    public decimal Cost { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Vendor { get; private set; }
    public DateOnly? NextMaintenanceDate { get; private set; }
    public Guid? ExpenseId { get; private set; }
    public Guid? CreatedById { get; private set; }

    private AssetMaintenance() { }

    public static AssetMaintenance Create(Guid assetId, DateOnly maintenanceDate,
        decimal cost, string description, string? vendor = null,
        DateOnly? nextMaintenanceDate = null, Guid? createdById = null)
    {
        if (cost < 0) throw new ArgumentException("Cost cannot be negative.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.");
        return new AssetMaintenance
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            MaintenanceDate = maintenanceDate,
            Cost = cost,
            Description = description.Trim(),
            Vendor = vendor,
            NextMaintenanceDate = nextMaintenanceDate,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetExpenseId(Guid expenseId)
    {
        ExpenseId = expenseId;
        Touch();
    }
}
