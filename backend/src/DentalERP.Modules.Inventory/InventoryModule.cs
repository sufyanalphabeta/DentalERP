using DentalERP.Modules.Inventory.Endpoints;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.Modules.Inventory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InventoryModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(InventoryModule).Assembly);

        services.AddScoped<IItemCodeGenerator, ItemCodeGenerator>();
        services.AddScoped<IMovementNumberGenerator, MovementNumberGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapInventoryModule(this IEndpointRouteBuilder app)
    {
        app.MapInventoryEndpoints();
        return app;
    }
}
