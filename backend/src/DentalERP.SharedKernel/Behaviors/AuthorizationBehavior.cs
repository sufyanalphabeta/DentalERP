using System.Reflection;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.SharedKernel.Behaviors;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permission) : Attribute
{
    public string Permission { get; } = permission;
}

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly MethodInfo? GenericFailureMethod =
        typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var permissions = typeof(TRequest)
            .GetCustomAttributes(typeof(RequirePermissionAttribute), true)
            .Cast<RequirePermissionAttribute>()
            .Select(a => a.Permission)
            .ToList();

        if (permissions.Count == 0) return await next();

        if (!currentUser.IsAuthenticated)
            return CreateFailure(Error.Unauthorized());

        if (!permissions.All(p => currentUser.HasPermission(p)))
            return CreateFailure(Error.Forbidden());

        return await next();
    }

    private static TResponse CreateFailure(Error error)
    {
        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && GenericFailureMethod is not null)
        {
            var bound = GenericFailureMethod.MakeGenericMethod(resultType.GetGenericArguments()[0]);
            return (TResponse)bound.Invoke(null, [error])!;
        }
        return (TResponse)Result.Failure(error);
    }
}
