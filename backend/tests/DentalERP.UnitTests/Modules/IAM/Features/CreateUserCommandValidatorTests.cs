using DentalERP.Modules.IAM.Features.Users.CreateUser;
using FluentValidation.TestHelper;
using Xunit;

namespace DentalERP.UnitTests.Modules.IAM.Features;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var cmd = new CreateUserCommand("john_doe", "StrongPass1", "John Doe", null, null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("j", "StrongPass1", "John")]
    [InlineData("john", "weak", "John")]
    [InlineData("john", "nodigits", "John")]
    [InlineData("john", "NOLOWERCASE1", "")]
    public void Invalid_Command_ShouldFail(string username, string password, string fullName)
    {
        var cmd = new CreateUserCommand(username, password, fullName, null, null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void InvalidUsername_WithSpaces_ShouldFail()
    {
        var cmd = new CreateUserCommand("john doe", "StrongPass1", "John Doe", null, null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void InvalidEmail_ShouldFail()
    {
        var cmd = new CreateUserCommand("john", "StrongPass1", "John", "notanemail", null, []);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
