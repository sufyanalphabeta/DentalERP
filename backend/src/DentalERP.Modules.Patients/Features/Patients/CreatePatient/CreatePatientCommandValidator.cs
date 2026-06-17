using FluentValidation;

namespace DentalERP.Modules.Patients.Features.Patients.CreatePatient;

public sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    private static readonly string[] ValidGenders = ["Male", "Female"];
    private static readonly string[] ValidBloodTypes = ["A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"];

    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم مطلوب.")
            .MaximumLength(200).WithMessage("الاسم يجب أن لا يتجاوز 200 حرف.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
            .MaximumLength(20).WithMessage("رقم الهاتف يجب أن لا يتجاوز 20 حرف.");

        RuleFor(x => x.Gender)
            .Must(g => g == null || ValidGenders.Contains(g))
            .WithMessage("الجنس يجب أن يكون Male أو Female.");

        RuleFor(x => x.BloodType)
            .Must(bt => bt == null || ValidBloodTypes.Contains(bt))
            .WithMessage("فصيلة الدم غير صالحة.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("البريد الإلكتروني غير صالح.");

        RuleFor(x => x.DateOfBirth)
            .Must(d => d == null || d < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("تاريخ الميلاد يجب أن يكون في الماضي.");
    }
}
