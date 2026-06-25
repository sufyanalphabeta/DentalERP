using MediatR;

namespace DentalERP.SharedKernel.Documents;

/// <summary>
/// Cross-module query: any module can send this to get company branding settings.
/// Handler lives in DentalERP.Modules.IAM and is resolved by MediatR at runtime.
/// </summary>
public sealed record GetCompanySettingsQuery : IRequest<CompanySettings>;
