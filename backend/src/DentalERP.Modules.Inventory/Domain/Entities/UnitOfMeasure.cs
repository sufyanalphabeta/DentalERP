using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class UnitOfMeasure : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public string? Abbreviation { get; private set; }

    private UnitOfMeasure() { }

    public static UnitOfMeasure Create(string name, string? nameAr = null, string? abbreviation = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameAr = nameAr,
            Abbreviation = abbreviation,
            CreatedAt = DateTime.UtcNow
        };
}
