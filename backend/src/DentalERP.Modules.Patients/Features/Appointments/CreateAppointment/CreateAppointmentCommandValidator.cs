using FluentValidation;

namespace DentalERP.Modules.Patients.Features.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("المريض مطلوب.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("الطبيب مطلوب.");
        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("تاريخ الموعد يجب أن يكون في المستقبل.");
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 240)
            .WithMessage("مدة الموعد يجب أن تكون بين 5 و 240 دقيقة.");
    }
}
