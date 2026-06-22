using DentalERP.Modules.Assets.Endpoints;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.Modules.Assets.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Assets;

public static class AssetsModule
{
    public static IServiceCollection AddAssetsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AssetsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssetsModule).Assembly));
        services.AddScoped<IAssetTagGenerator, AssetTagGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapAssetsModule(this IEndpointRouteBuilder app)
    {
        app.MapAssetsEndpoints();
        return app;
    }
}
