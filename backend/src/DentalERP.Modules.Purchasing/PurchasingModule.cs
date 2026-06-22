using DentalERP.Modules.Purchasing.Endpoints;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Purchasing;

public static class PurchasingModule
{
    public static IServiceCollection AddPurchasingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PurchasingDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PurchasingModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(PurchasingModule).Assembly);

        services.AddScoped<ISupplierCodeGenerator, SupplierCodeGenerator>();
        services.AddScoped<IPONumberGenerator, PONumberGenerator>();
        services.AddScoped<IGRNumberGenerator, GRNumberGenerator>();
        services.AddScoped<IReturnNumberGenerator, ReturnNumberGenerator>();
        services.AddScoped<ISupplierPaymentNumberGenerator, SupplierPaymentNumberGenerator>();
        services.AddScoped<IPINumberGenerator, PINumberGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapPurchasingModule(this IEndpointRouteBuilder app)
    {
        app.MapSupplierEndpoints();
        app.MapPurchasingEndpoints();
        app.MapPurchaseInvoiceEndpoints();
        return app;
    }
}
