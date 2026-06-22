using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseReturn : BaseEntity
{
    public string ReturnNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid? PoId { get; private set; }
    public DateOnly ReturnDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Draft";
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedById { get; private set; }

    private readonly List<PurchaseReturnItem> _items = [];
    public IReadOnlyList<PurchaseReturnItem> Items => _items;

    private PurchaseReturn() { }

    public static PurchaseReturn Create(string returnNumber, Guid supplierId, DateOnly returnDate,
        string reason, Guid? poId = null, string? notes = null, Guid? createdById = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Return reason is required.");

        return new PurchaseReturn
        {
            Id = Guid.NewGuid(),
            ReturnNumber = returnNumber,
            SupplierId = supplierId,
            PoId = poId,
            ReturnDate = returnDate,
            Reason = reason,
            Status = "Draft",
            Notes = notes,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(PurchaseReturnItem item)
    {
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.TotalCost);
    }

    public Result Confirm()
    {
        if (Status != "Draft")
            return Result.Failure(new Error("PurchaseReturn.InvalidStatus", $"Cannot confirm return in {Status} status."));
        if (!_items.Any())
            return Result.Failure(new Error("PurchaseReturn.NoItems", "Return must have at least one item."));

        Status = "Confirmed";
        Touch();
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != "Confirmed")
            return Result.Failure(new Error("PurchaseReturn.InvalidStatus", $"Cannot complete return in {Status} status. Must be Confirmed first."));

        Status = "Completed";
        Touch();
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == "Cancelled")
            return Result.Failure(new Error("PurchaseReturn.AlreadyCancelled", "Return is already cancelled."));
        if (Status == "Completed")
            return Result.Failure(new Error("PurchaseReturn.CannotCancelCompleted", "Cannot cancel a completed return."));

        Status = "Cancelled";
        Touch();
        return Result.Success();
    }
}
