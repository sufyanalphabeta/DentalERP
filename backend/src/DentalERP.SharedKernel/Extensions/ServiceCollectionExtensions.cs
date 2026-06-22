using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.SharedKernel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddSingleton<IFileStorageService, NullFileStorageService>();
        return services;
    }
}
