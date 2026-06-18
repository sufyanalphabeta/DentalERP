using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Expenses.Domain.Entities;

public sealed class Expense : BaseEntity
{
    public static readonly string[] ValidCostCenters =
        ["GENERAL", "CLINIC", "LABORATORY", "RADIOLOGY", "TRAINING", "ADMINISTRATION"];

    public string ExpenseNumber { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public string CostCenter { get; private set; } = "GENERAL";
    public DateOnly ExpenseDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? RelatedModule { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public Guid? VaultId { get; private set; }
    public string? Notes { get; private set; }
    public string? AttachmentKey { get; private set; }
    public string? AttachmentName { get; private set; }
    public Guid? CreatedById { get; private set; }

    private Expense() { }

    public static Expense Create(string expenseNumber, Guid? categoryId, string costCenter,
        DateOnly expenseDate, decimal amount, string description,
        string? relatedModule = null, Guid? relatedEntityId = null,
        Guid? vaultId = null, string? notes = null, Guid? createdById = null)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.");
        if (!ValidCostCenters.Contains(costCenter))
            throw new ArgumentException($"Invalid cost center: {costCenter}");

        return new Expense
        {
            Id = Guid.NewGuid(),
            ExpenseNumber = expenseNumber,
            CategoryId = categoryId,
            CostCenter = costCenter,
            ExpenseDate = expenseDate,
            Amount = amount,
            Description = description.Trim(),
            RelatedModule = relatedModule,
            RelatedEntityId = relatedEntityId,
            VaultId = vaultId,
            Notes = notes,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(Guid? categoryId, string costCenter, DateOnly expenseDate,
        decimal amount, string description, string? notes)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        if (!ValidCostCenters.Contains(costCenter))
            throw new ArgumentException($"Invalid cost center: {costCenter}");

        CategoryId = categoryId;
        CostCenter = costCenter;
        ExpenseDate = expenseDate;
        Amount = amount;
        Description = description;
        Notes = notes;
        Touch();
    }

    public void SetAttachment(string key, string name)
    {
        AttachmentKey = key;
        AttachmentName = name;
        Touch();
    }

    public void Delete() => SoftDelete();
}
