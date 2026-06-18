using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.SaveRadiologyReport;

public sealed record SaveRadiologyReportCommand(
    Guid OrderId,
    string ReportText,
    Guid ReportedById
) : IRequest<Result>;
