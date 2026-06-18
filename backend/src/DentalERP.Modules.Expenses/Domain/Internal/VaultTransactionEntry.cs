namespace DentalERP.Modules.Expenses.Domain.Internal;

// Write model for vault_transactions table (owned by Financial module).
// ExpensesDbContext maps to this table for writing expense deductions atomically.
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
