using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GenerateAssetRegister;

public sealed record GenerateAssetRegisterQuery(
    Guid? CategoryId,
    string? Status
) : IRequest<Result<byte[]>>;
