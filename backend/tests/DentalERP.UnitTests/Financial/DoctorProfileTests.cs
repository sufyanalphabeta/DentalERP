using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class DoctorProfileTests
{
    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_DefaultMethodIsPercentageOfService()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid());
        profile.CommissionMethod.Should().Be("percentage_of_service");
    }

    [Fact]
    public void Create_DefaultCommissionValueIsZero()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid());
        profile.DefaultCommissionValue.Should().Be(0m);
    }

    [Fact]
    public void Create_StoresUserId()
    {
        var userId = Guid.NewGuid();
        var profile = DoctorProfile.Create(userId);
        profile.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_StoresCustomMethodAndValue()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid(), "fixed_amount", 100m);
        profile.CommissionMethod.Should().Be("fixed_amount");
        profile.DefaultCommissionValue.Should().Be(100m);
    }

    [Fact]
    public void Create_SetsUpdatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var profile = DoctorProfile.Create(Guid.NewGuid());
        profile.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public void Update_ChangesMethodAndValue()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid(), "percentage_of_service", 10m);
        profile.Update("fixed_amount", 200m);

        profile.CommissionMethod.Should().Be("fixed_amount");
        profile.DefaultCommissionValue.Should().Be(200m);
    }

    [Fact]
    public void Update_BumpsUpdatedAt()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid(), "percentage_of_service", 10m);
        var before = profile.UpdatedAt;

        // tiny delay to ensure timestamp differs
        System.Threading.Thread.Sleep(5);
        profile.Update("fixed_amount", 50m);

        profile.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Update_ToPercentageOfNetService_Succeeds()
    {
        var profile = DoctorProfile.Create(Guid.NewGuid());
        profile.Update("percentage_of_net_service", 20m);

        profile.CommissionMethod.Should().Be("percentage_of_net_service");
        profile.DefaultCommissionValue.Should().Be(20m);
    }

    // ── ValidMethods ─────────────────────────────────────────────

    [Fact]
    public void ValidMethods_ContainsThreeMethods()
    {
        DoctorProfile.ValidMethods.Should().HaveCount(3);
        DoctorProfile.ValidMethods.Should().Contain([
            "percentage_of_service",
            "fixed_amount",
            "percentage_of_net_service"
        ]);
    }
}
