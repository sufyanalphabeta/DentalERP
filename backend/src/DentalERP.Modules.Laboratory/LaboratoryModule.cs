using DentalERP.Modules.Laboratory.Endpoints;
using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.Modules.Laboratory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Laboratory;

public static class LaboratoryModule
{
    public static IServiceCollection AddLaboratoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LaboratoryDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LaboratoryModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(LaboratoryModule).Assembly);
        services.AddScoped<ILabOrderNumberGenerator, LabOrderNumberGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapLaboratoryModule(this IEndpointRouteBuilder app)
    {
        app.MapLabEndpoints();
        return app;
    }
}
