using DentalERP.Modules.IAM.Features.Auth.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DentalERP.UnitTests.Modules.IAM.Features;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var result = _validator.TestValidate(new LoginCommand("admin", "password123"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("admin", "")]
    [InlineData("admin", "12345")]
    public void Invalid_Command_ShouldFail(string username, string password)
    {
        var result = _validator.TestValidate(new LoginCommand(username, password));
        result.ShouldHaveAnyValidationError();
    }
}
