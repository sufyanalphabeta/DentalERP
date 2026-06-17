using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Patients.Domain.Entities;

public sealed class AppointmentType : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string NameAr { get; private set; } = default!;
    public int DefaultDurationMinutes { get; private set; }
    public string Color { get; private set; } = "#3B82F6";
    public bool IsActive { get; private set; } = true;

    private AppointmentType() { }

    public static AppointmentType Create(string name, string nameAr, int durationMinutes, string color = "#3B82F6")
        => new() { Name = name, NameAr = nameAr, DefaultDurationMinutes = durationMinutes, Color = color };
}
