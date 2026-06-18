using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public class VaultTransferTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var result = VaultTransfer.Create("TRF-2026-000001", fromId, toId, 500m, null, Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
        result.Value!.Amount.Should().Be(500m);
        result.Value.FromVaultId.Should().Be(fromId);
        result.Value.ToVaultId.Should().Be(toId);
    }

    [Fact]
    public void Create_SameVault_Fails()
    {
        var sameId = Guid.NewGuid();
        var result = VaultTransfer.Create("TRF-2026-000001", sameId, sameId, 500m, null, Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("VaultTransfer.SameVault");
    }

    [Fact]
    public void Create_NegativeAmount_Fails()
    {
        var result = VaultTransfer.Create("TRF-2026-000001", Guid.NewGuid(), Guid.NewGuid(), -100m, null, Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("VaultTransfer.InvalidAmount");
    }

    [Fact]
    public void Create_ZeroAmount_Fails()
    {
        var result = VaultTransfer.Create("TRF-2026-000001", Guid.NewGuid(), Guid.NewGuid(), 0m, null, Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
    }
}
