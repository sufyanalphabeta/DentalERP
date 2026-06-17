namespace DentalERP.SharedKernel.Results;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("General.NullValue", "A null value was provided.");

    public static Error NotFound(string resource) => new($"{resource}.NotFound", $"{resource} was not found.");
    public static Error Validation(string field, string message) => new($"Validation.{field}", message);
    public static Error Conflict(string resource) => new($"{resource}.Conflict", $"{resource} already exists.");
    public static Error Unauthorized() => new("Auth.Unauthorized", "You are not authorized.");
    public static Error Forbidden() => new("Auth.Forbidden", "Access denied.");
}
