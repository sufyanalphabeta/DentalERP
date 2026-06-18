using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Expenses.Domain.Entities;

public sealed class ExpenseCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ExpenseCategory() { }

    public static ExpenseCategory Create(string name, string? nameAr = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        return new ExpenseCategory
        {
            Id = Guid.NewGuid(), Name = name.Trim(), NameAr = nameAr,
            Description = description, IsActive = true, CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        Name = name.Trim(); NameAr = nameAr; Description = description;
        Touch();
    }

    public void Delete() => SoftDelete();
}
