using DentalERP.Modules.Clinical.Endpoints;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Clinical;

public static class ClinicalModule
{
    public static IServiceCollection AddClinicalModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ClinicalDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ClinicalModule).Assembly));

        services.AddValidatorsFromAssembly(typeof(ClinicalModule).Assembly);

        services.AddScoped<ITimelineService, TimelineService>();

        return services;
    }

    public static IEndpointRouteBuilder MapClinicalModule(this IEndpointRouteBuilder app)
    {
        app.MapChartEndpoints();
        app.MapTreatmentPlanEndpoints();
        app.MapProcedureEndpoints();
        app.MapMediaEndpoints();
        app.MapAssignmentEndpoints();
        app.MapTimelineEndpoints();
        return app;
    }
}
