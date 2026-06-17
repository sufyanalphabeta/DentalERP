using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.IAM.Domain.Entities;

public sealed class Role : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public static Role Create(string name, string? description = null, bool isSystem = false)
        => new() { Name = name, Description = description, IsSystem = isSystem };

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        Touch();
    }

    public void AddPermission(Guid permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId)) return;
        _rolePermissions.Add(new RolePermission { RoleId = Id, PermissionId = permissionId });
        Touch();
    }

    public void ClearPermissions()
    {
        _rolePermissions.Clear();
        Touch();
    }
}

public sealed class RolePermission
{
    public Guid RoleId { get; init; }
    public Guid PermissionId { get; init; }

    public Role Role { get; init; } = default!;
    public Permission Permission { get; init; } = default!;
}
