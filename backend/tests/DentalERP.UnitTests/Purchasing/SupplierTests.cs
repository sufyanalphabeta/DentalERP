using DentalERP.Modules.Purchasing.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Purchasing;

public sealed class SupplierTests
{
    [Fact]
    public void Create_ValidSupplier_SetsProperties()
    {
        var supplier = Supplier.Create("SUP-000001", "MedCo", category: "Medical");

        supplier.SupplierCode.Should().Be("SUP-000001");
        supplier.Name.Should().Be("MedCo");
        supplier.Category.Should().Be("Medical");
        supplier.IsActive.Should().BeTrue();
        supplier.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_InvalidCategory_Throws()
    {
        var act = () => Supplier.Create("SUP-000002", "Bad Supplier", category: "InvalidCat");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NullCategory_Allowed()
    {
        var supplier = Supplier.Create("SUP-000003", "General Co", category: null);
        supplier.Category.Should().BeNull();
    }

    [Theory]
    [InlineData("Medical")]
    [InlineData("Equipment")]
    [InlineData("General")]
    [InlineData("Lab")]
    [InlineData("Radiology")]
    [InlineData("Pharma")]
    public void Create_AllValidCategories_Succeed(string category)
    {
        var supplier = Supplier.Create("SUP-000004", "Any Supplier", category: category);
        supplier.Category.Should().Be(category);
    }

    [Fact]
    public void Update_ChangesName()
    {
        var supplier = Supplier.Create("SUP-000005", "Old Name");
        supplier.Update("New Name", null, "Lab", null, null, null, null, 30, 0, null, 0);
        supplier.Name.Should().Be("New Name");
        supplier.Category.Should().Be("Lab");
    }

    [Fact]
    public void Update_InvalidCategory_Throws()
    {
        var supplier = Supplier.Create("SUP-000006", "Supplier");
        var act = () => supplier.Update("Supplier", null, "BadCat", null, null, null, null, 30, 0, null, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var supplier = Supplier.Create("SUP-000007", "Active Supplier");
        supplier.Deactivate();
        supplier.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var supplier = Supplier.Create("SUP-000008", "Supplier");
        supplier.Deactivate();
        supplier.Activate();
        supplier.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedTrue()
    {
        var supplier = Supplier.Create("SUP-000009", "Doomed Supplier");
        supplier.Delete();
        supplier.IsDeleted.Should().BeTrue();
        supplier.DeletedAt.Should().NotBeNull();
    }
}
