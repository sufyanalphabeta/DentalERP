using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class PatientMediaTests
{
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_DefaultsToNotApproved()
    {
        var media = PatientMedia.Create(PatientId, UserId, "XRay", "xray.jpg", "patient-media/1/xray.jpg");

        media.IsApproved.Should().BeFalse();
        media.ApprovedById.Should().BeNull();
        media.ApprovedAt.Should().BeNull();
    }

    [Fact]
    public void Approve_SetsApprovalFields()
    {
        var media = PatientMedia.Create(PatientId, UserId, "OPG", "opg.jpg", "patient-media/1/opg.jpg");
        var approverId = Guid.NewGuid();

        media.Approve(approverId);

        media.IsApproved.Should().BeTrue();
        media.ApprovedById.Should().Be(approverId);
        media.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_SetsDeletedAt()
    {
        var media = PatientMedia.Create(PatientId, UserId, "Before", "before.jpg", "patient-media/1/before.jpg");

        media.SoftDelete();

        media.DeletedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Before")]
    [InlineData("After")]
    [InlineData("OPG")]
    [InlineData("CBCT")]
    [InlineData("XRay")]
    [InlineData("Document")]
    public void ValidMediaTypes_ContainsAllExpected(string mediaType)
    {
        PatientMedia.ValidMediaTypes.Should().Contain(mediaType);
    }

    [Fact]
    public void Create_WithIsRequired_SetsFlag()
    {
        var media = PatientMedia.Create(PatientId, UserId, "OPG", "opg.jpg", "path", isRequired: true);

        media.IsRequired.Should().BeTrue();
    }
}
