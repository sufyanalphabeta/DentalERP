using DentalERP.Modules.Laboratory.Domain.Entities;
using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CreateExternalLab;

public sealed class CreateExternalLabCommandHandler(LaboratoryDbContext db)
    : IRequestHandler<CreateExternalLabCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExternalLabCommand request, CancellationToken cancellationToken)
    {
        var lab = ExternalLab.Create(request.Name, request.ContactName, request.Phone, request.Email, request.Address, request.Notes);
        db.ExternalLabs.Add(lab);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(lab.Id);
    }
}
