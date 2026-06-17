using DentalERP.Modules.Patients.Domain.Entities;
using Xunit;

namespace DentalERP.UnitTests.Modules.Patients.Domain;

public class AppointmentEntityTests
{
    private static Guid PatientId => Guid.NewGuid();
    private static Guid DoctorId => Guid.NewGuid();

    [Fact]
    public void Create_SetsStatusScheduled()
    {
        var apt = Appointment.Create(PatientId, DoctorId, DateTime.UtcNow.AddHours(1), 30);
        Assert.Equal(AppointmentStatus.Scheduled, apt.Status);
    }

    [Fact]
    public void Confirm_ChangesStatusToConfirmed()
    {
        var apt = Appointment.Create(PatientId, DoctorId, DateTime.UtcNow.AddHours(1), 30);
        apt.Confirm();
        Assert.Equal(AppointmentStatus.Confirmed, apt.Status);
    }

    [Fact]
    public void Cancel_SetsReasonAndStatus()
    {
        var apt = Appointment.Create(PatientId, DoctorId, DateTime.UtcNow.AddHours(1), 30);
        apt.Cancel("لا يوجد وقت");
        Assert.Equal(AppointmentStatus.Cancelled, apt.Status);
        Assert.Equal("لا يوجد وقت", apt.CancellationReason);
    }

    [Fact]
    public void EndsAt_CalculatesCorrectly()
    {
        var start = new DateTime(2026, 6, 17, 10, 0, 0, DateTimeKind.Utc);
        var apt = Appointment.Create(PatientId, DoctorId, start, 45);
        Assert.Equal(start.AddMinutes(45), apt.EndsAt);
    }

    [Fact]
    public void Reschedule_ResetsStatusToScheduled()
    {
        var apt = Appointment.Create(PatientId, DoctorId, DateTime.UtcNow.AddHours(1), 30);
        apt.Confirm();
        var newTime = DateTime.UtcNow.AddDays(1);
        apt.Reschedule(newTime);
        Assert.Equal(AppointmentStatus.Scheduled, apt.Status);
        Assert.Equal(newTime, apt.ScheduledAt);
    }
}
