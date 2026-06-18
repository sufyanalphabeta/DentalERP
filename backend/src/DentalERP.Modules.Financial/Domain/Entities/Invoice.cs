using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class Invoice
{
    public static readonly string[] ValidStatuses =
        ["Draft", "Confirmed", "PartiallyPaid", "Paid", "Cancelled"];

    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public string Status { get; private set; } = "Draft";
    public decimal Subtotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal Remaining => TotalAmount - PaidAmount;
    public string Currency { get; private set; } = "LYD";
    public string? Notes { get; private set; }
    public string? CancelledReason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyList<InvoiceItem> Items => _items;

    private Invoice() { }

    public static Invoice Create(
        string invoiceNumber,
        Guid patientId,
        Guid doctorId,
        Guid? createdByUserId = null,
        string? notes = null,
        string currency = "LYD")
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            PatientId = patientId,
            DoctorId = doctorId,
            Status = "Draft",
            Currency = currency,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

    public void AddItem(InvoiceItem item)
    {
        _items.Add(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        Subtotal = _items.Sum(i => i.UnitPrice * i.Quantity);
        DiscountTotal = _items.Sum(i => i.Discount);
        TotalAmount = _items.Sum(i => i.Total);
        UpdatedAt = DateTime.UtcNow;
    }

    public Result Confirm()
    {
        if (Status != "Draft")
            return Result.Failure(new Error("Invoice.NotDraft", "يمكن تأكيد الفواتير في حالة مسودة فقط"));
        if (!_items.Any())
            return Result.Failure(new Error("Invoice.NoItems", "لا يمكن تأكيد فاتورة بدون بنود"));
        Status = "Confirmed";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status is "Paid" or "Cancelled")
            return Result.Failure(new Error("Invoice.CannotCancel", "لا يمكن إلغاء فاتورة مدفوعة أو ملغاة"));
        Status = "Cancelled";
        CancelledReason = reason;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ApplyPayment(decimal amount)
    {
        if (Status is "Cancelled")
            return Result.Failure(new Error("Invoice.Cancelled", "لا يمكن إضافة دفعة لفاتورة ملغاة"));
        if (Status == "Draft")
            return Result.Failure(new Error("Invoice.Draft", "يجب تأكيد الفاتورة قبل تسجيل الدفعة"));
        if (amount <= 0)
            return Result.Failure(new Error("Payment.InvalidAmount", "مبلغ الدفعة يجب أن يكون أكبر من صفر"));
        if (amount > Remaining)
            return Result.Failure(new Error("Payment.ExceedsRemaining", "مبلغ الدفعة يتجاوز المبلغ المتبقي"));

        PaidAmount += amount;
        Status = PaidAmount >= TotalAmount ? "Paid" : "PartiallyPaid";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
