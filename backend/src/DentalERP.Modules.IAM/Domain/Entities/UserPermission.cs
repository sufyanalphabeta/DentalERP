namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class UserPermission
{
    public Guid UserId { get; init; }
    public Guid PermissionId { get; init; }
    public string GrantType { get; set; } = "Grant"; // "Grant" | "Deny"

    public User User { get; init; } = default!;
    public Permission Permission { get; init; } = default!;
}
