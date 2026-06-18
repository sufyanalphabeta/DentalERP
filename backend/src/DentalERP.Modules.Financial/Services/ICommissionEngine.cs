using DentalERP.Modules.Financial.Domain.Entities;

namespace DentalERP.Modules.Financial.Services;

public interface ICommissionEngine
{
    Task<CommissionRecord?> CalculateAsync(
        Guid doctorId,
        Guid invoiceId,
        Guid paymentId,
        decimal paidAmount,
        decimal labCost = 0,
        CancellationToken ct = default);
}
