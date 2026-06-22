using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.Modules.Expenses.Features.CreateExpense;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.CreateAssetMaintenance;

internal sealed class CreateAssetMaintenanceCommandHandler : IRequestHandler<CreateAssetMaintenanceCommand, Result<Guid>>
{
    private readonly AssetsDbContext _db;
    private readonly IMediator _mediator;

    public CreateAssetMaintenanceCommandHandler(AssetsDbContext db, IMediator mediator)
    {
        _db = db; _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(CreateAssetMaintenanceCommand request, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        if (asset is null) return Result.Failure<Guid>(Error.NotFound("Asset"));

        // Auto-create expense for this maintenance
        Guid? expenseId = null;
        if (request.Cost > 0)
        {
            var expenseResult = await _mediator.Send(new CreateExpenseCommand(
                CategoryId: request.CostCategoryId,
                CostCenter: "GENERAL",
                ExpenseDate: request.MaintenanceDate,
                Amount: request.Cost,
                Description: $"Asset Maintenance: {asset.AssetTag} - {request.Description}",
                RelatedModule: "Asset",
                RelatedEntityId: asset.Id,
                VaultId: request.VaultId,
                Notes: request.Vendor,
                CreatedById: request.CreatedById
            ), ct);

            if (expenseResult.IsSuccess) expenseId = expenseResult.Value;
        }

        var maintenance = AssetMaintenance.Create(
            request.AssetId, request.MaintenanceDate, request.Cost,
            request.Description, request.Vendor, request.CreatedById);

        if (expenseId.HasValue) maintenance.SetExpenseId(expenseId.Value);

        // Set asset under maintenance if currently active
        if (asset.Status == "Active") asset.SetUnderMaintenance();

        _db.AssetMaintenances.Add(maintenance);

        _db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "Asset", EntityId = asset.Id, Action = "Maintenance",
            PerformedById = request.CreatedById,
            Details = $"Asset {asset.AssetTag} maintenance recorded. Cost: {request.Cost}",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(maintenance.Id);
    }
}
