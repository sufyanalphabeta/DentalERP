using DentalERP.Modules.Assets.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Assets;

public class AssetTests
{
    private static Asset ValidAsset() =>
        Asset.Create("AST-000001", "Laptop Dell XPS 15", null,
            DateOnly.FromDateTime(DateTime.Today), 2500m, "Server Room");

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var asset = ValidAsset();
        asset.AssetTag.Should().Be("AST-000001");
        asset.Name.Should().Be("Laptop Dell XPS 15");
        asset.Status.Should().Be("Active");
        asset.PurchaseCost.Should().Be(2500m);
    }

    [Fact]
    public void Create_ShouldUppercaseTag()
    {
        var asset = Asset.Create("ast-000001", "Test", null, null, null, null);
        asset.AssetTag.Should().Be("AST-000001");
    }

    [Fact]
    public void Create_WithEmptyTag_ShouldThrow()
    {
        var act = () => Asset.Create("", "Name", null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => Asset.Create("AST-001", "", null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetUnderMaintenance_FromActive_ShouldSucceed()
    {
        var asset = ValidAsset();
        var result = asset.SetUnderMaintenance();
        result.IsSuccess.Should().BeTrue();
        asset.Status.Should().Be("UnderMaintenance");
    }

    [Fact]
    public void SetActive_FromMaintenance_ShouldSucceed()
    {
        var asset = ValidAsset();
        asset.SetUnderMaintenance();
        var result = asset.SetActive();
        result.IsSuccess.Should().BeTrue();
        asset.Status.Should().Be("Active");
    }

    [Fact]
    public void Dispose_FromActive_ShouldSucceed()
    {
        var asset = ValidAsset();
        var result = asset.Dispose();
        result.IsSuccess.Should().BeTrue();
        asset.Status.Should().Be("Disposed");
        asset.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Dispose_AlreadyDisposed_ShouldFail()
    {
        var asset = ValidAsset();
        asset.Dispose();
        var result = asset.Dispose();
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Asset.AlreadyDisposed");
    }

    [Fact]
    public void SetUnderMaintenance_WhenDisposed_ShouldFail()
    {
        var asset = ValidAsset();
        asset.Dispose();
        var result = asset.SetUnderMaintenance();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SetActive_WhenDisposed_ShouldFail()
    {
        var asset = ValidAsset();
        asset.Dispose();
        var result = asset.SetActive();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldChangeProperties()
    {
        var asset = ValidAsset();
        asset.Update("Updated Name", Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 3000m, "Clinic", "notes");
        asset.Name.Should().Be("Updated Name");
        asset.PurchaseCost.Should().Be(3000m);
        asset.Location.Should().Be("Clinic");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var a1 = ValidAsset();
        var a2 = Asset.Create("AST-000002", "Asset 2", null, null, null, null);
        a1.Id.Should().NotBe(a2.Id);
    }

    [Fact]
    public void Create_WithNullPurchaseCost_ShouldNotThrow()
    {
        var asset = Asset.Create("AST-001", "Test", null, null, null, null);
        asset.PurchaseCost.Should().BeNull();
    }

    [Fact]
    public void ValidStatuses_ContainsThreeStatuses()
    {
        Asset.ValidStatuses.Should().HaveCount(3);
        Asset.ValidStatuses.Should().Contain(["Active", "UnderMaintenance", "Disposed"]);
    }
}

