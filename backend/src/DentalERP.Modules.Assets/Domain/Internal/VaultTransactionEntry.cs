namespace DentalERP.Modules.Assets.Domain.Internal;

// Write model for vault_transactions table — Assets module writes expense deductions via maintenance.
internal sealed class VaultTransactionEntry
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public string TransactionType { get; set; } = "general_payment";
    public decimal Amount { get; set; }
    public string Direction { get; set; } = "out";
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
