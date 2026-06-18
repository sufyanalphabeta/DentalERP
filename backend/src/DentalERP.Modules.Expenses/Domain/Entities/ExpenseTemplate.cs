using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Expenses.Domain.Entities;

public sealed class ExpenseTemplate : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public string CostCenter { get; private set; } = "GENERAL";
    public decimal? DefaultAmount { get; private set; }
    public string? Notes { get; private set; }

    private ExpenseTemplate() { }

    public static ExpenseTemplate Create(string name, Guid? categoryId, string costCenter,
        decimal? defaultAmount = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        return new ExpenseTemplate
        {
            Id = Guid.NewGuid(), Name = name.Trim(),
            CategoryId = categoryId, CostCenter = costCenter,
            DefaultAmount = defaultAmount, Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}
