using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrder : BaseEntity
{
    public string PoNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public string Status { get; private set; } = "Draft";
    public DateOnly OrderDate { get; private set; }
    public DateOnly? ExpectedDate { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? CreatedById { get; private set; }

    private readonly List<PurchaseOrderItem> _items = [];
    public IReadOnlyList<PurchaseOrderItem> Items => _items;

    private PurchaseOrder() { }

    public static PurchaseOrder Create(string poNumber, Guid supplierId, DateOnly orderDate,
        DateOnly? expectedDate = null, decimal discountAmount = 0, string? notes = null, Guid? createdById = null)
        => new()
        {
            Id = Guid.NewGuid(),
            PoNumber = poNumber,
            SupplierId = supplierId,
            Status = "Draft",
            OrderDate = orderDate,
            ExpectedDate = expectedDate,
            DiscountAmount = discountAmount,
            Notes = notes,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

    public void AddItem(PurchaseOrderItem item)
    {
        _items.Add(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        Subtotal = _items.Sum(i => i.TotalCost);
        TotalAmount = Math.Max(0, Subtotal - DiscountAmount);
        Touch();
    }

    public Result Approve(Guid approvedById)
    {
        if (Status != "Draft")
            return Result.Failure(new Error("PurchaseOrder.InvalidStatus", $"Cannot approve PO in {Status} status."));
        if (!_items.Any())
            return Result.Failure(new Error("PurchaseOrder.NoItems", "Purchase order must have at least one item."));

        Status = "Approved";
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }

    public Result MarkSent()
    {
        if (Status != "Approved")
            return Result.Failure(new Error("PurchaseOrder.InvalidStatus", $"PO must be Approved before marking Sent. Current: {Status}"));
        Status = "Sent";
        Touch();
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == "PartiallyReceived" || Status == "FullyReceived" || Status == "Closed")
            return Result.Failure(new Error("PurchaseOrder.CannotCancel", "Cannot cancel PO that has received goods."));
        if (Status == "Cancelled")
            return Result.Failure(new Error("PurchaseOrder.AlreadyCancelled", "PO is already cancelled."));
        Status = "Cancelled";
        Touch();
        return Result.Success();
    }

    public void UpdateReceiptStatus()
    {
        if (!_items.Any()) return;
        var allFull = _items.All(i => i.QuantityReceived >= i.QuantityOrdered);
        var anyReceived = _items.Any(i => i.QuantityReceived > 0);

        Status = allFull ? "FullyReceived" : anyReceived ? "PartiallyReceived" : Status;
        Touch();
    }

    public void Close()
    {
        if (Status == "FullyReceived")
        {
            Status = "Closed";
            Touch();
        }
    }
}
