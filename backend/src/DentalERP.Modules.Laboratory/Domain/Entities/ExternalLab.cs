namespace DentalERP.Modules.Laboratory.Domain.Entities;

public sealed class ExternalLab
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private ExternalLab() { }

    public static ExternalLab Create(string name, string? contactName = null, string? phone = null,
        string? email = null, string? address = null, string? notes = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactName = contactName,
            Phone = phone,
            Email = email,
            Address = address,
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, string? contactName, string? phone, string? email, string? address, string? notes)
    {
        Name = name;
        ContactName = contactName;
        Phone = phone;
        Email = email;
        Address = address;
        Notes = notes;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
