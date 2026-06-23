using DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceClaim;
using DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceCompany;
using DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaimById;
using DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaims;
using DentalERP.Modules.Financial.Features.Insurance.GetInsuranceCompanies;
using DentalERP.Modules.Financial.Features.Insurance.RecordInsurancePayment;
using DentalERP.Modules.Financial.Features.Insurance.RejectInsuranceClaim;
using DentalERP.Modules.Financial.Features.Insurance.SubmitInsuranceClaim;
using DentalERP.Modules.Financial.Features.Insurance.UpdateInsuranceCompany;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class InsuranceEndpoints
{
    public static IEndpointRouteBuilder MapInsuranceEndpoints(this IEndpointRouteBuilder app)
    {
        var ins = app.MapGroup("/api/insurance");

        ins.MapGet("/companies", async (IMediator mediator, bool activeOnly = true) =>
        {
            var result = await mediator.Send(new GetInsuranceCompaniesQuery(activeOnly));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Companies.View");

        ins.MapPost("/companies", async (IMediator mediator, CreateInsuranceCompanyCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/insurance/companies/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Companies.Create");

        ins.MapPut("/companies/{id:guid}", async (IMediator mediator, Guid id, UpdateInsuranceCompanyRequest req) =>
        {
            var result = await mediator.Send(new UpdateInsuranceCompanyCommand(
                id, req.Name, req.NameAr, req.ContactPerson, req.Phone, req.Email, req.DefaultCoveragePercent, req.IsActive));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Companies.Edit");

        ins.MapGet("/claims", async (IMediator mediator,
            Guid? patientId, Guid? insuranceCompanyId, string? status, int page = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetInsuranceClaimsQuery(patientId, insuranceCompanyId, status, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Claims.View");

        ins.MapGet("/claims/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetInsuranceClaimByIdQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        }).RequirePermission("Insurance.Claims.View");

        ins.MapPost("/claims", async (IMediator mediator, CreateInsuranceClaimCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/insurance/claims/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Claims.Create");

        ins.MapPost("/claims/{id:guid}/submit", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new SubmitInsuranceClaimCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Claims.Approve");

        ins.MapPost("/claims/{id:guid}/payment", async (IMediator mediator, Guid id, InsurancePaymentRequest req) =>
        {
            var result = await mediator.Send(new RecordInsurancePaymentCommand(id, req.Amount, req.ReferenceNumber, req.Notes, req.ReceivedById, req.VaultId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Claims.Approve");

        ins.MapPost("/claims/{id:guid}/reject", async (IMediator mediator, Guid id, RejectRequest req) =>
        {
            var result = await mediator.Send(new RejectInsuranceClaimCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Insurance.Claims.Cancel");

        return app;
    }

    private sealed record InsurancePaymentRequest(decimal Amount, string? ReferenceNumber, string? Notes, Guid ReceivedById, Guid? VaultId = null);
    private sealed record RejectRequest(string Reason);
    private sealed record UpdateInsuranceCompanyRequest(string Name, string? NameAr, string? ContactPerson, string? Phone, string? Email, decimal DefaultCoveragePercent, bool IsActive);
}
