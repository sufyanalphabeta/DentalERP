using DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceClaim;
using DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceCompany;
using DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaims;
using DentalERP.Modules.Financial.Features.Insurance.GetInsuranceCompanies;
using DentalERP.Modules.Financial.Features.Insurance.RecordInsurancePayment;
using DentalERP.Modules.Financial.Features.Insurance.RejectInsuranceClaim;
using DentalERP.Modules.Financial.Features.Insurance.SubmitInsuranceClaim;
using DentalERP.Modules.Financial.Features.Vaults.CreateVaultTransfer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class InsuranceEndpoints
{
    public static IEndpointRouteBuilder MapInsuranceEndpoints(this IEndpointRouteBuilder app)
    {
        var ins = app.MapGroup("/api/insurance").RequireAuthorization();

        ins.MapGet("/companies", async (IMediator mediator, bool activeOnly = true) =>
        {
            var result = await mediator.Send(new GetInsuranceCompaniesQuery(activeOnly));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        ins.MapPost("/companies", async (IMediator mediator, CreateInsuranceCompanyCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/insurance/companies/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        ins.MapGet("/claims", async (IMediator mediator,
            Guid? patientId, Guid? insuranceCompanyId, string? status, int page = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetInsuranceClaimsQuery(patientId, insuranceCompanyId, status, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        ins.MapPost("/claims", async (IMediator mediator, CreateInsuranceClaimCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/insurance/claims/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        ins.MapPost("/claims/{id:guid}/submit", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new SubmitInsuranceClaimCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        ins.MapPost("/claims/{id:guid}/payment", async (IMediator mediator, Guid id, InsurancePaymentRequest req) =>
        {
            var result = await mediator.Send(new RecordInsurancePaymentCommand(id, req.Amount, req.ReferenceNumber, req.Notes, req.ReceivedById));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        ins.MapPost("/claims/{id:guid}/reject", async (IMediator mediator, Guid id, RejectRequest req) =>
        {
            var result = await mediator.Send(new RejectInsuranceClaimCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        var vaults = app.MapGroup("/api/vaults").RequireAuthorization();

        vaults.MapPost("/transfer", async (IMediator mediator, CreateVaultTransferCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/vaults/transfers/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        return app;
    }

    private sealed record InsurancePaymentRequest(decimal Amount, string? ReferenceNumber, string? Notes, Guid ReceivedById);
    private sealed record RejectRequest(string Reason);
}
