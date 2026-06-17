namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? UserId { get; init; }
    public string Username { get; init; } = "system";
    public string EntityName { get; init; } = default!;
    public string EntityId { get; init; } = default!;
    public string Action { get; init; } = default!;    // Created | Updated | Deleted | Login | Logout
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
