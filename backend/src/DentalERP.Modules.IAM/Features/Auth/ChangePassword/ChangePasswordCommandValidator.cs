using FluentValidation;

namespace DentalERP.Modules.IAM.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty()
            .Matches(@"^\d{4,8}$").WithMessage("كلمة المرور يجب أن تكون 4 إلى 8 أرقام فقط.");
        RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.NewPassword)
            .WithMessage("كلمة المرور وتأكيدها غير متطابقتين.");
    }
}
