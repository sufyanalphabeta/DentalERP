namespace DentalERP.Modules.Laboratory.Domain.Entities;

public sealed class LabClient
{
    public static readonly string[] ValidClientTypes = ["Doctor", "Clinic", "ExternalClient"];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ClientType { get; private set; } = "ExternalClient";
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private LabClient() { }

    public static LabClient Create(string name, string clientType = "ExternalClient",
        string? phone = null, string? email = null, string? address = null, string? notes = null)
    {
        if (!ValidClientTypes.Contains(clientType))
            throw new ArgumentException($"Invalid client type '{clientType}'. Valid types: {string.Join(", ", ValidClientTypes)}");

        return new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            ClientType = clientType,
            Phone = phone,
            Email = email,
            Address = address,
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string clientType, string? phone, string? email, string? address, string? notes)
    {
        Name = name;
        ClientType = clientType;
        Phone = phone;
        Email = email;
        Address = address;
        Notes = notes;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
