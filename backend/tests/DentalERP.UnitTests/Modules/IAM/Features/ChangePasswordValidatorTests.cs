using DentalERP.Modules.IAM.Features.Auth.ChangePassword;
using FluentValidation.TestHelper;

namespace DentalERP.UnitTests.Modules.IAM.Features;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var cmd = new ChangePasswordCommand("OldPass123", "NewPass456", "NewPass456");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MismatchedPasswords_ShouldFail()
    {
        var cmd = new ChangePasswordCommand("OldPass123", "NewPass456", "Different789");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void WeakPassword_NoDigits_ShouldFail()
    {
        var cmd = new ChangePasswordCommand("OldPass123", "NewPassword", "NewPassword");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ShortPassword_ShouldFail()
    {
        var cmd = new ChangePasswordCommand("OldPass123", "Ab1", "Ab1");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
