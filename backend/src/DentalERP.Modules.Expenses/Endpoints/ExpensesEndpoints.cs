using DentalERP.Modules.Expenses.Features.CreateExpense;
using DentalERP.Modules.Expenses.Features.CreateExpenseCategory;
using DentalERP.Modules.Expenses.Features.DeleteExpense;
using DentalERP.Modules.Expenses.Features.GenerateExpenseReport;
using DentalERP.Modules.Expenses.Features.GetExpenseCategories;
using DentalERP.Modules.Expenses.Features.GetExpenseDetail;
using DentalERP.Modules.Expenses.Features.GetExpenses;
using DentalERP.Modules.Expenses.Features.GetExpenseVoucher;
using DentalERP.Modules.Expenses.Features.UpdateExpense;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Expenses.Endpoints;

internal static class ExpensesEndpoints
{
    internal static void MapExpensesEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/expenses").RequireAuthorization();

        // Expense Categories
        grp.MapGet("/categories", async (bool? activeOnly, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExpenseCategoriesQuery(activeOnly ?? false));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetExpenseCategories");

        grp.MapPost("/categories", async (CreateExpenseCategoryCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/expenses/categories/{result.Value}", result.Value)
                : Results.Conflict(result.Error);
        }).WithName("CreateExpenseCategory");

        // Expenses
        grp.MapGet("/", async (string? costCenter, Guid? categoryId, DateOnly? dateFrom,
            DateOnly? dateTo, string? relatedModule, int page, int pageSize, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExpensesQuery(
                costCenter, categoryId, dateFrom, dateTo, relatedModule,
                page < 1 ? 1 : page, pageSize < 1 ? 50 : pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetExpenses");

        grp.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExpenseDetailQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        }).WithName("GetExpenseDetail");

        grp.MapPost("/", async (CreateExpenseCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/expenses/{result.Value}", result.Value)
                : Results.BadRequest(result.Error);
        }).WithName("CreateExpense");

        grp.MapPut("/{id:guid}", async (Guid id, UpdateExpenseRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateExpenseCommand(
                id, req.CategoryId, req.CostCenter, req.ExpenseDate,
                req.Amount, req.Description, req.Notes));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithName("UpdateExpense");

        grp.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteExpenseCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
        }).WithName("DeleteExpense");

        // Single-expense PDF voucher
        grp.MapGet("/{id:guid}/voucher", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExpenseVoucherQuery(id));
            return result.IsSuccess
                ? Results.File(result.Value, "application/pdf", $"expense-voucher-{id}.pdf")
                : Results.NotFound(result.Error);
        }).WithName("GetExpenseVoucher");

        // PDF Report
        grp.MapGet("/report/pdf", async (DateOnly dateFrom, DateOnly dateTo,
            string? costCenter, Guid? categoryId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GenerateExpenseReportQuery(dateFrom, dateTo, costCenter, categoryId));
            return result.IsSuccess
                ? Results.File(result.Value, "application/pdf", $"expense-report-{dateFrom:yyyyMMdd}-{dateTo:yyyyMMdd}.pdf")
                : Results.BadRequest(result.Error);
        }).WithName("GetExpenseReportPdf");
    }
}

internal sealed record UpdateExpenseRequest(
    Guid? CategoryId, string CostCenter, DateOnly ExpenseDate,
    decimal Amount, string Description, string? Notes
);

