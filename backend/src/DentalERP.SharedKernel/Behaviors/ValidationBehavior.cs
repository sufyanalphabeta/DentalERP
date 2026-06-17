using System.Reflection;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.SharedKernel.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly MethodInfo? GenericFailureMethod =
        typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var errors = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
            .ToList();

        if (errors.Count == 0) return await next();

        var error = new Error("Validation.Failed", string.Join("; ", errors));

        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && GenericFailureMethod is not null)
        {
            var bound = GenericFailureMethod.MakeGenericMethod(resultType.GetGenericArguments()[0]);
            return (TResponse)bound.Invoke(null, [error])!;
        }

        return (TResponse)Result.Failure(error);
    }
}
