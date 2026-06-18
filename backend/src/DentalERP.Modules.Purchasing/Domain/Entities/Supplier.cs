using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class Supplier : BaseEntity
{
    public static readonly string[] ValidCategories =
        ["Medical", "Equipment", "General", "Lab", "Radiology", "Pharma"];

    public string SupplierCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public string? Category { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public int PaymentTermsDays { get; private set; } = 30;
    public decimal CreditLimit { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private readonly List<SupplierItem> _items = [];
    public IReadOnlyList<SupplierItem> Items => _items;

    private Supplier() { }

    public static Supplier Create(string supplierCode, string name, string? nameAr = null,
        string? category = null, string? contactPerson = null, string? phone = null,
        string? email = null, string? address = null, int paymentTermsDays = 30,
        decimal creditLimit = 0, string? notes = null)
    {
        if (category != null && !ValidCategories.Contains(category))
            throw new ArgumentException($"Invalid supplier category: {category}. Valid: {string.Join(", ", ValidCategories)}");

        return new Supplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = supplierCode,
            Name = name,
            NameAr = nameAr,
            Category = category,
            ContactPerson = contactPerson,
            Phone = phone,
            Email = email,
            Address = address,
            PaymentTermsDays = paymentTermsDays,
            CreditLimit = creditLimit,
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, string? category, string? contactPerson,
        string? phone, string? email, string? address, int paymentTermsDays, decimal creditLimit, string? notes)
    {
        if (category != null && !ValidCategories.Contains(category))
            throw new ArgumentException($"Invalid supplier category: {category}");

        Name = name;
        NameAr = nameAr;
        Category = category;
        ContactPerson = contactPerson;
        Phone = phone;
        Email = email;
        Address = address;
        PaymentTermsDays = paymentTermsDays;
        CreditLimit = creditLimit;
        Notes = notes;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }
    public void Delete()     => SoftDelete();
}
