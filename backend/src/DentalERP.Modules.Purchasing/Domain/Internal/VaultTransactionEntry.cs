namespace DentalERP.Modules.Purchasing.Domain.Internal;

// Write model for vault_transactions table (owned by Financial module).
// PurchasingDbContext maps to this table for writing supplier payment deductions.
// No EF migration generated from this context for vault_transactions.
internal sealed class VaultTransactionEntry
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
