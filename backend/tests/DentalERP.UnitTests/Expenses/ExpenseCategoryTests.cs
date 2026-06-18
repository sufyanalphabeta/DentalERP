using DentalERP.Modules.Expenses.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Expenses;

public class ExpenseCategoryTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        var cat = ExpenseCategory.Create("Utilities");
        cat.Name.Should().Be("Utilities");
        cat.IsActive.Should().BeTrue();
        cat.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllFields_ShouldSetAllProperties()
    {
        var cat = ExpenseCategory.Create("Supplies", "مستلزمات", "Office supplies");
        cat.NameAr.Should().Be("مستلزمات");
        cat.Description.Should().Be("Office supplies");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => ExpenseCategory.Create("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrow()
    {
        var act = () => ExpenseCategory.Create("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var cat = ExpenseCategory.Create("  Rent  ");
        cat.Name.Should().Be("Rent");
    }

    [Fact]
    public void Create_ShouldGenerateId()
    {
        var cat = ExpenseCategory.Create("Test");
        cat.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Update_ShouldChangeName()
    {
        var cat = ExpenseCategory.Create("Old");
        cat.Update("New", "جديد", "desc");
        cat.Name.Should().Be("New");
        cat.NameAr.Should().Be("جديد");
        cat.Description.Should().Be("desc");
    }

    [Fact]
    public void Delete_ShouldSoftDelete()
    {
        var cat = ExpenseCategory.Create("Test");
        cat.Delete();
        cat.DeletedAt.Should().NotBeNull();
        cat.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Create_HasCorrectCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var cat = ExpenseCategory.Create("Test");
        cat.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var cat = ExpenseCategory.Create("Test");
        var before = cat.CreatedAt;
        cat.Update("New", null, null);
        cat.UpdatedAt.Should().NotBeNull();
    }
}
