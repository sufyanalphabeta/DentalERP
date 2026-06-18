using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class DentalChartEntryTests
{
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidCondition_SetsIsCurrent()
    {
        var entry = DentalChartEntry.Create(PatientId, 16, "Caries", UserId);

        entry.IsCurrent.Should().BeTrue();
        entry.Condition.Should().Be("Caries");
        entry.ToothId.Should().Be(16);
    }

    [Fact]
    public void Create_WithSurface_SetsSurface()
    {
        var entry = DentalChartEntry.Create(PatientId, 16, "Filled", UserId, surface: "M");

        entry.Surface.Should().Be("M");
    }

    [Fact]
    public void Create_WithNullSurface_RepresentsWholeTooth()
    {
        var entry = DentalChartEntry.Create(PatientId, 16, "Crown", UserId, surface: null);

        entry.Surface.Should().BeNull();
    }

    [Fact]
    public void MarkSuperseded_SetsIsCurrentFalse()
    {
        var entry = DentalChartEntry.Create(PatientId, 16, "Caries", UserId);

        entry.MarkSuperseded();

        entry.IsCurrent.Should().BeFalse();
    }

    [Theory]
    [InlineData("Healthy")]
    [InlineData("Caries")]
    [InlineData("Filled")]
    [InlineData("Missing")]
    [InlineData("Extracted")]
    [InlineData("Crown")]
    [InlineData("Bridge")]
    [InlineData("Implant")]
    [InlineData("RootCanal")]
    [InlineData("Fracture")]
    [InlineData("Impacted")]
    [InlineData("Sensitive")]
    [InlineData("Mobility")]
    [InlineData("Other")]
    public void ValidConditions_AreAllIncluded(string condition)
    {
        DentalChartEntry.ValidConditions.Should().Contain(condition);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("D")]
    [InlineData("B")]
    [InlineData("L")]
    [InlineData("O")]
    public void ValidSurfaces_AreAllIncluded(string surface)
    {
        DentalChartEntry.ValidSurfaces.Should().Contain(surface);
    }

    [Fact]
    public void Create_SetsRecordedAt_ToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entry = DentalChartEntry.Create(PatientId, 16, "Caries", UserId);

        entry.RecordedAt.Should().BeAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithAppointmentId_SetsLink()
    {
        var appointmentId = Guid.NewGuid();
        var entry = DentalChartEntry.Create(PatientId, 16, "Filled", UserId, appointmentId: appointmentId);

        entry.AppointmentId.Should().Be(appointmentId);
    }
}
