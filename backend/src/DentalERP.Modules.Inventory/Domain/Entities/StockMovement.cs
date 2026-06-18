using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class StockMovement
{
    public static readonly string[] ValidMovementTypes =
        ["PurchaseReceipt", "ManualIssue", "LabConsumption", "RadiologyConsumption",
         "Adjustment", "WriteOff", "SupplierReturn", "Transfer"];

    public static readonly string[] ValidDestinationTypes =
        ["Clinic", "Lab", "Radiology", "Doctor", "Other"];

    public Guid Id { get; private set; }
    public string MovementNumber { get; private set; } = string.Empty;
    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? BatchId { get; private set; }
    public string MovementType { get; private set; } = string.Empty;
    public string Direction { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal? UnitCost { get; private set; }
    public decimal? TotalCost { get; private set; }
    public string? DestinationType { get; private set; }
    public Guid? DestinationId { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public bool IsNegativeStock { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    public static Result<StockMovement> Create(
        string movementNumber,
        Guid itemId,
        Guid warehouseId,
        string movementType,
        string direction,
        decimal quantity,
        Guid? batchId = null,
        decimal? unitCost = null,
        string? destinationType = null,
        Guid? destinationId = null,
        Guid? referenceId = null,
        string? referenceType = null,
        bool isNegativeStock = false,
        string? notes = null,
        Guid? createdById = null)
    {
        if (quantity <= 0)
            return Result.Failure<StockMovement>(new Error("StockMovement.InvalidQuantity", "Quantity must be greater than zero."));

        if (!ValidMovementTypes.Contains(movementType))
            return Result.Failure<StockMovement>(new Error("StockMovement.InvalidType", $"Invalid movement type: {movementType}"));

        if (direction != "in" && direction != "out")
            return Result.Failure<StockMovement>(new Error("StockMovement.InvalidDirection", "Direction must be 'in' or 'out'."));

        if (movementType == "ManualIssue" && string.IsNullOrWhiteSpace(destinationType))
            return Result.Failure<StockMovement>(new Error("StockMovement.DestinationRequired", "Destination type is required for manual issues."));

        if (destinationType != null && !ValidDestinationTypes.Contains(destinationType))
            return Result.Failure<StockMovement>(new Error("StockMovement.InvalidDestination", $"Invalid destination type: {destinationType}"));

        // Auto-set destination for consumption movements
        if (movementType == "LabConsumption")        destinationType = "Lab";
        if (movementType == "RadiologyConsumption")  destinationType = "Radiology";

        return Result.Success(new StockMovement
        {
            Id = Guid.NewGuid(),
            MovementNumber = movementNumber,
            ItemId = itemId,
            WarehouseId = warehouseId,
            BatchId = batchId,
            MovementType = movementType,
            Direction = direction,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = unitCost.HasValue ? unitCost * quantity : null,
            DestinationType = destinationType,
            DestinationId = destinationId,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsNegativeStock = isNegativeStock,
            Notes = notes,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        });
    }
}
