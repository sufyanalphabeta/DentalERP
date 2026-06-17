using DentalERP.Modules.Patients.Domain.Entities;
using Xunit;

namespace DentalERP.UnitTests.Modules.Patients.Domain;

public class PatientEntityTests
{
    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var patient = Patient.Create("P2026-00001", "محمد علي", "0501234567",
            gender: "Male", nationalId: "1234567890");

        Assert.Equal("P2026-00001", patient.FileNumber);
        Assert.Equal("محمد علي", patient.FullName);
        Assert.Equal("0501234567", patient.Phone);
        Assert.Equal("Male", patient.Gender);
        Assert.True(patient.IsActive);
        Assert.Null(patient.DeletedAt);
    }

    [Fact]
    public void Delete_SetsDeletedAt()
    {
        var patient = Patient.Create("P2026-00002", "فاطمة", "0507654321");
        patient.Delete();

        Assert.NotNull(patient.DeletedAt);
        Assert.True(patient.IsDeleted);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var patient = Patient.Create("P2026-00003", "أحمد", "0509999999");
        patient.Deactivate();

        Assert.False(patient.IsActive);
    }

    [Fact]
    public void Update_ChangesFullNameAndPhone()
    {
        var patient = Patient.Create("P2026-00004", "علي", "0501111111");
        patient.Update("علي محمد", "0502222222");

        Assert.Equal("علي محمد", patient.FullName);
        Assert.Equal("0502222222", patient.Phone);
        Assert.NotNull(patient.UpdatedAt);
    }

    [Fact]
    public void Age_WithDateOfBirth_ReturnsCorrectAge()
    {
        var dob = new DateOnly(2000, 1, 1);
        var patient = Patient.Create("P2026-00005", "سارة", "0503333333", dateOfBirth: dob);

        Assert.NotNull(patient.Age);
        Assert.InRange(patient.Age!.Value, 25, 27);
    }
}
