using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Patients.Domain.Entities;

public sealed class Patient : BaseEntity
{
    public string FileNumber { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public DateOnly? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string Phone { get; private set; } = default!;
    public string? Phone2 { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? NationalId { get; private set; }
    public string? BloodType { get; private set; }
    public string? Allergies { get; private set; }
    public string? ChronicDiseases { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? InsuranceCompanyId { get; private set; }

    private Patient() { }

    public static Patient Create(
        string fileNumber,
        string fullName,
        string phone,
        DateOnly? dateOfBirth = null,
        string? gender = null,
        string? phone2 = null,
        string? email = null,
        string? address = null,
        string? nationalId = null,
        string? bloodType = null,
        string? allergies = null,
        string? chronicDiseases = null,
        string? notes = null,
        Guid? insuranceCompanyId = null)
    {
        var patient = new Patient
        {
            FileNumber = fileNumber,
            FullName = fullName,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Phone2 = phone2,
            Email = email,
            Address = address,
            NationalId = nationalId,
            BloodType = bloodType,
            Allergies = allergies,
            ChronicDiseases = chronicDiseases,
            Notes = notes,
            InsuranceCompanyId = insuranceCompanyId
        };
        return patient;
    }

    public void Update(
        string fullName,
        string phone,
        DateOnly? dateOfBirth = null,
        string? gender = null,
        string? phone2 = null,
        string? email = null,
        string? address = null,
        string? nationalId = null,
        string? bloodType = null,
        string? allergies = null,
        string? chronicDiseases = null,
        string? notes = null,
        Guid? insuranceCompanyId = null)
    {
        FullName = fullName;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Phone2 = phone2;
        Email = email;
        Address = address;
        NationalId = nationalId;
        BloodType = bloodType;
        Allergies = allergies;
        ChronicDiseases = chronicDiseases;
        Notes = notes;
        InsuranceCompanyId = insuranceCompanyId;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
    public void Delete() { SoftDelete(); Touch(); }

    public int? Age => DateOfBirth.HasValue
        ? (int)((DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - DateOfBirth.Value.DayNumber) / 365.25)
        : null;
}
