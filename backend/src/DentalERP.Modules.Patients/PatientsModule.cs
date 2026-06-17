using DentalERP.Modules.Patients.Endpoints;
using DentalERP.Modules.Patients.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Patients;

public static class PatientsModule
{
    public static IServiceCollection AddPatientsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PatientsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(PatientsModule).Assembly));

        services.AddValidatorsFromAssembly(typeof(PatientsModule).Assembly);

        return services;
    }

    public static IEndpointRouteBuilder MapPatientsModule(this IEndpointRouteBuilder app)
    {
        app.MapPatientsEndpoints();
        app.MapAppointmentsEndpoints();
        app.MapQueueEndpoints();
        return app;
    }
}
