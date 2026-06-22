using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Laboratory.Domain.Entities;

public sealed class LabOrder
{
    public static readonly string[] ValidStatuses =
        ["Draft", "Sent", "InProgress", "ResultReceived", "Completed", "Cancelled"];

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid? LabId { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? ProcedureId { get; private set; }
    public bool IsExternal { get; private set; }
    public string Status { get; private set; } = "Draft";
    public string? Description { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateOnly? ExpectedAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public string Currency { get; private set; } = "LYD";
    public string? Notes { get; private set; }
    public string? CancelledReason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<LabOrderItem> _items = [];
    public IReadOnlyList<LabOrderItem> Items => _items;

    private readonly List<LabResult> _results = [];
    public IReadOnlyList<LabResult> Results => _results;

    private LabOrder() { }

    public static LabOrder Create(
        string orderNumber,
        Guid patientId,
        Guid? doctorId,
        Guid? labId = null,
        Guid? clientId = null,
        Guid? procedureId = null,
        string? description = null,
        DateOnly? expectedAt = null,
        string? notes = null,
        Guid? createdByUserId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            PatientId = patientId,
            DoctorId = doctorId,
            LabId = labId,
            ClientId = clientId,
            ProcedureId = procedureId,
            IsExternal = clientId.HasValue,
            Status = "Draft",
            Description = description,
            ExpectedAt = expectedAt,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

    public void AddItem(LabOrderItem item)
    {
        _items.Add(item);
        RecalculateCosts();
    }

    public void RecalculateCosts()
    {
        TotalCost = _items.Sum(i => i.TotalCost);
        UpdatedAt = DateTime.UtcNow;
    }

    public Result Send()
    {
        if (Status != "Draft")
            return Result.Failure(new Error("LabOrder.NotDraft", "يمكن إرسال الطلبات في حالة مسودة فقط"));
        Status = "Sent";
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkInProgress()
    {
        if (Status != "Sent")
            return Result.Failure(new Error("LabOrder.NotSent", "يجب إرسال الطلب أولاً"));
        Status = "InProgress";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RecordResult(LabResult result)
    {
        if (Status is "Completed" or "Cancelled")
            return Result.Failure(new Error("LabOrder.Terminal", "لا يمكن إضافة نتيجة لطلب مكتمل أو ملغى"));
        _results.Add(result);
        Status = "ResultReceived";
        ReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != "ResultReceived")
            return Result.Failure(new Error("LabOrder.NoResult", "يجب استلام النتيجة قبل الإكمال"));
        Status = "Completed";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status != "Draft")
            return Result.Failure(new Error("LabOrder.CannotCancel", "يمكن إلغاء الطلبات في حالة مسودة فقط"));
        Status = "Cancelled";
        CancelledReason = reason;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void SetRevenue(decimal revenue)
    {
        TotalRevenue = revenue;
        UpdatedAt = DateTime.UtcNow;
    }
}
