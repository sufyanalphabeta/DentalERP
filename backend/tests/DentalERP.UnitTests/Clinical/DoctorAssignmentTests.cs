using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class DoctorAssignmentTests
{
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid DoctorId = Guid.NewGuid();
    private static readonly Guid NewDoctorId = Guid.NewGuid();

    [Fact]
    public void Create_DefaultsToActiveStatus()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);

        assignment.Status.Should().Be("Active");
        assignment.CanView.Should().BeTrue();
        assignment.CanEdit.Should().BeTrue();
    }

    [Fact]
    public void Transfer_FromActive_CreatesNewAssignment()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);

        var newAssignment = assignment.Transfer(NewDoctorId, "تخصص أعلى");

        assignment.Status.Should().Be("Transferred");
        assignment.CanEdit.Should().BeFalse();
        assignment.CanView.Should().BeTrue();
        assignment.TransferredToId.Should().Be(NewDoctorId);

        newAssignment.DoctorId.Should().Be(NewDoctorId);
        newAssignment.Status.Should().Be("Active");
        newAssignment.CanEdit.Should().BeTrue();
    }

    [Fact]
    public void Transfer_FromCompleted_ThrowsInvalidOperation()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);
        assignment.Complete();

        assignment.Invoking(a => a.Transfer(NewDoctorId))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_FromActive_SetsCorrectFlags()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);

        assignment.Complete();

        assignment.Status.Should().Be("Completed");
        assignment.CanEdit.Should().BeFalse();
        assignment.CanView.Should().BeTrue();
        assignment.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_SetsAllFlagsFalse()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);
        assignment.Complete();

        assignment.Close();

        assignment.Status.Should().Be("Closed");
        assignment.CanView.Should().BeFalse();
        assignment.CanEdit.Should().BeFalse();
    }

    [Fact]
    public void Complete_FromTransferred_ThrowsInvalidOperation()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);
        assignment.Transfer(NewDoctorId);

        assignment.Invoking(a => a.Complete())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Transfer_SetsTransferReasonAndTimestamp()
    {
        var assignment = DoctorAssignment.Create(PatientId, DoctorId);

        assignment.Transfer(NewDoctorId, reason: "طارئ طبي");

        assignment.TransferReason.Should().Be("طارئ طبي");
        assignment.TransferredAt.Should().NotBeNull();
    }
}
