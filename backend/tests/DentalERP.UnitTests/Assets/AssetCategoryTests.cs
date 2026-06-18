using DentalERP.Modules.Assets.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Assets;

public class AssetCategoryTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        var cat = AssetCategory.Create("Computers");
        cat.Name.Should().Be("Computers");
        cat.IsActive.Should().BeTrue();
        cat.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllFields_ShouldSetAllProperties()
    {
        var cat = AssetCategory.Create("Furniture", "أثاث", "Office furniture");
        cat.NameAr.Should().Be("أثاث");
        cat.Description.Should().Be("Office furniture");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => AssetCategory.Create("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var cat = AssetCategory.Create("  Equipment  ");
        cat.Name.Should().Be("Equipment");
    }

    [Fact]
    public void Delete_ShouldSoftDelete()
    {
        var cat = AssetCategory.Create("Test");
        cat.Delete();
        cat.IsDeleted.Should().BeTrue();
        cat.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldChangeProperties()
    {
        var cat = AssetCategory.Create("Old");
        cat.Update("New", "جديد", "desc");
        cat.Name.Should().Be("New");
        cat.NameAr.Should().Be("جديد");
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var cat = AssetCategory.Create("Test");
        cat.CreatedAt.Should().BeAfter(before);
    }
}
