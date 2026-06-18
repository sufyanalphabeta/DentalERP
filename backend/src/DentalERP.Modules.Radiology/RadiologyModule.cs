using DentalERP.Modules.Radiology.Endpoints;
using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.Modules.Radiology.Services;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Radiology;

public static class RadiologyModule
{
    public static IServiceCollection AddRadiologyModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RadiologyDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RadiologyModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(RadiologyModule).Assembly);
        services.AddScoped<IRadiologyOrderNumberGenerator, RadiologyOrderNumberGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapRadiologyModule(this IEndpointRouteBuilder app)
    {
        app.MapRadiologyEndpoints();
        return app;
    }
}
