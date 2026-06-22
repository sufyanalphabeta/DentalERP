using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaimById;

public sealed class GetInsuranceClaimByIdQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInsuranceClaimByIdQuery, Result<InsuranceClaimDetailDto>>
{
    public async Task<Result<InsuranceClaimDetailDto>> Handle(GetInsuranceClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = await db.InsuranceClaims
            .Include(c => c.InsuranceCompany)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (claim is null)
            return Result.Failure<InsuranceClaimDetailDto>(new Error("InsuranceClaim.NotFound", "المطالبة غير موجودة"));

        var patientName = await db.PatientNames
            .Where(p => p.Id == claim.PatientId)
            .Select(p => p.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? "—";

        var payments = claim.Payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new InsurancePaymentDto(p.Id, p.Amount, p.ReferenceNumber, p.PaymentDate))
            .ToList();

        return Result.Success(new InsuranceClaimDetailDto(
            claim.Id,
            claim.ClaimNumber,
            claim.Status,
            claim.InsuranceCompanyId,
            claim.InsuranceCompany.Name,
            claim.PatientId,
            patientName,
            claim.InvoiceId,
            claim.ClaimedAmount,
            claim.PaidAmount,
            claim.CoveragePercent,
            claim.RejectionReason,
            claim.Notes,
            claim.ClaimDate,
            claim.SubmittedAt,
            payments));
    }
}
