using DentalERP.Modules.Expenses.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Expenses;

public class ExpenseTests
{
    private static Expense ValidExpense() =>
        Expense.Create("EXP-2026-000001", null, "GENERAL",
            DateOnly.FromDateTime(DateTime.Today), 150.00m, "Office supplies");

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var expense = ValidExpense();
        expense.ExpenseNumber.Should().Be("EXP-2026-000001");
        expense.Amount.Should().Be(150.00m);
        expense.CostCenter.Should().Be("GENERAL");
        expense.Description.Should().Be("Office supplies");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => Expense.Create("EXP-001", null, "GENERAL",
            DateOnly.FromDateTime(DateTime.Today), 0m, "Test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Expense.Create("EXP-001", null, "GENERAL",
            DateOnly.FromDateTime(DateTime.Today), -10m, "Test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrow()
    {
        var act = () => Expense.Create("EXP-001", null, "GENERAL",
            DateOnly.FromDateTime(DateTime.Today), 100m, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidCostCenter_ShouldThrow()
    {
        var act = () => Expense.Create("EXP-001", null, "INVALID",
            DateOnly.FromDateTime(DateTime.Today), 100m, "Test");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("GENERAL")]
    [InlineData("CLINIC")]
    [InlineData("LABORATORY")]
    [InlineData("RADIOLOGY")]
    [InlineData("TRAINING")]
    [InlineData("ADMINISTRATION")]
    public void Create_WithAllValidCostCenters_ShouldSucceed(string costCenter)
    {
        var expense = Expense.Create("EXP-001", null, costCenter, DateOnly.FromDateTime(DateTime.Today), 100m, "Test");
        expense.CostCenter.Should().Be(costCenter);
    }

    [Fact]
    public void Create_WithRelatedModule_ShouldSetRelatedFields()
    {
        var entityId = Guid.NewGuid();
        var expense = Expense.Create("EXP-001", null, "GENERAL", DateOnly.FromDateTime(DateTime.Today), 100m,
            "Test", relatedModule: "Asset", relatedEntityId: entityId);
        expense.RelatedModule.Should().Be("Asset");
        expense.RelatedEntityId.Should().Be(entityId);
    }

    [Fact]
    public void Update_ShouldChangeAmount()
    {
        var expense = ValidExpense();
        expense.Update(null, "CLINIC", DateOnly.FromDateTime(DateTime.Today), 250m, "Updated desc", "some notes");
        expense.Amount.Should().Be(250m);
        expense.CostCenter.Should().Be("CLINIC");
        expense.Description.Should().Be("Updated desc");
    }

    [Fact]
    public void Update_WithZeroAmount_ShouldThrow()
    {
        var expense = ValidExpense();
        var act = () => expense.Update(null, "GENERAL", DateOnly.FromDateTime(DateTime.Today), 0m, "desc", null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetAttachment_ShouldSetKeyAndName()
    {
        var expense = ValidExpense();
        expense.SetAttachment("expenses/2026/receipt.pdf", "receipt.pdf");
        expense.AttachmentKey.Should().Be("expenses/2026/receipt.pdf");
        expense.AttachmentName.Should().Be("receipt.pdf");
    }

    [Fact]
    public void Delete_ShouldSoftDelete()
    {
        var expense = ValidExpense();
        expense.Delete();
        expense.IsDeleted.Should().BeTrue();
        expense.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var e1 = ValidExpense();
        var e2 = ValidExpense();
        e1.Id.Should().NotBe(e2.Id);
    }

    [Fact]
    public void Create_WithVaultId_ShouldSetVaultId()
    {
        var vaultId = Guid.NewGuid();
        var expense = Expense.Create("EXP-001", null, "GENERAL", DateOnly.FromDateTime(DateTime.Today), 100m,
            "Test", vaultId: vaultId);
        expense.VaultId.Should().Be(vaultId);
    }
}

