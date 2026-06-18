namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid? ProcedureId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public string? ServiceCode { get; private set; }
    public short Quantity { get; private set; } = 1;
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private InvoiceItem() { }

    public static InvoiceItem Create(
        Guid invoiceId,
        string serviceName,
        decimal unitPrice,
        short quantity = 1,
        decimal discount = 0,
        Guid? procedureId = null,
        string? serviceCode = null)
    {
        var total = (unitPrice * quantity) - discount;
        return new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            ServiceName = serviceName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Discount = discount,
            Total = total < 0 ? 0 : total,
            ProcedureId = procedureId,
            ServiceCode = serviceCode,
            CreatedAt = DateTime.UtcNow
        };
    }
}
