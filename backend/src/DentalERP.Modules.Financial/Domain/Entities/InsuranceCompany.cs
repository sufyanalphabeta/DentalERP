namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InsuranceCompany
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? NameAr { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public decimal DefaultCoveragePercent { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private InsuranceCompany() { }

    public static InsuranceCompany Create(string name, string? nameAr, string? contactPerson,
        string? phone, string? email, decimal defaultCoveragePercent)
    {
        return new InsuranceCompany
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameAr = nameAr,
            ContactPerson = contactPerson,
            Phone = phone,
            Email = email,
            DefaultCoveragePercent = defaultCoveragePercent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, string? contactPerson,
        string? phone, string? email, decimal defaultCoveragePercent)
    {
        Name = name;
        NameAr = nameAr;
        ContactPerson = contactPerson;
        Phone = phone;
        Email = email;
        DefaultCoveragePercent = defaultCoveragePercent;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
