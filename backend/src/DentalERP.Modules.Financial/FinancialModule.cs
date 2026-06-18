using DentalERP.Modules.Financial.Endpoints;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.Modules.Financial.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Financial;

public static class FinancialModule
{
    public static IServiceCollection AddFinancialModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<FinancialDbContext>(opts =>
            opts.UseNpgsql(connectionString));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FinancialModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(FinancialModule).Assembly);

        services.AddScoped<ICommissionEngine, CommissionEngine>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddScoped<IInsuranceClaimNumberGenerator, InsuranceClaimNumberGenerator>();

        return services;
    }

    public static IEndpointRouteBuilder MapFinancialModule(this IEndpointRouteBuilder app)
    {
        app.MapServiceEndpoints();
        app.MapInvoiceEndpoints();
        app.MapPaymentEndpoints();
        app.MapTreasuryEndpoints();
        app.MapInstallmentEndpoints();
        app.MapCommissionEndpoints();
        app.MapInsuranceEndpoints();
        return app;
    }
}
