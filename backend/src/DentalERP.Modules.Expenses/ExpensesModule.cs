using DentalERP.Modules.Expenses.Endpoints;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.Modules.Expenses.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentalERP.Modules.Expenses;

public static class ExpensesModule
{
    public static IServiceCollection AddExpensesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExpensesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IExpenseNumberGenerator, ExpenseNumberGenerator>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ExpensesModule).Assembly));

        return services;
    }

    public static IEndpointRouteBuilder MapExpensesModule(this IEndpointRouteBuilder app)
    {
        app.MapExpensesEndpoints();
        return app;
    }
}
