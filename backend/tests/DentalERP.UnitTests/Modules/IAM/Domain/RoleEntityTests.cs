using DentalERP.Modules.IAM.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Modules.IAM.Domain;

public class RoleEntityTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var role = Role.Create("Admin", "مدير النظام");
        role.Name.Should().Be("Admin");
        role.Description.Should().Be("مدير النظام");
        role.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void AddPermission_ShouldNotAddDuplicate()
    {
        var role = Role.Create("Editor");
        var permId = Guid.NewGuid();

        role.AddPermission(permId);
        role.AddPermission(permId);

        role.RolePermissions.Should().HaveCount(1);
    }

    [Fact]
    public void ClearPermissions_ShouldRemoveAll()
    {
        var role = Role.Create("Editor");
        role.AddPermission(Guid.NewGuid());
        role.AddPermission(Guid.NewGuid());

        role.ClearPermissions();

        role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldChangeName()
    {
        var role = Role.Create("Old Name");
        role.Update("New Name", "وصف جديد");
        role.Name.Should().Be("New Name");
        role.UpdatedAt.Should().NotBeNull();
    }
}
