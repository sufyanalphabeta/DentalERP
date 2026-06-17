using DentalERP.Modules.IAM.Endpoints;
using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.Modules.IAM.Infrastructure.Services;
using DentalERP.SharedKernel.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.IAM;

public static class IAMModule
{
    public static IServiceCollection AddIAMModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IAMDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npg => npg.MigrationsHistoryTable("__ef_migrations_history", "public")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<JwtService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<AuditService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAMModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(IAMModule).Assembly);

        return services;
    }

    public static IEndpointRouteBuilder MapIAMEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapUsersEndpoints();
        app.MapRolesEndpoints();
        app.MapSettingsEndpoints();
        return app;
    }
}
