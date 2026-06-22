using DentalERP.Modules.Expenses.Domain.Entities;
using DentalERP.Modules.Expenses.Domain.Internal;
using DentalERP.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Infrastructure;

internal sealed class ExpensesDbContext : DbContext
{
    public ExpensesDbContext(DbContextOptions<ExpensesDbContext> options) : base(options) { }

    internal DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    internal DbSet<ExpenseTemplate> ExpenseTemplates => Set<ExpenseTemplate>();
    internal DbSet<Expense> Expenses => Set<Expense>();
    internal DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    internal DbSet<VaultTransactionEntry> VaultTransactions => Set<VaultTransactionEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ExpenseCategory
        modelBuilder.Entity<ExpenseCategory>(e =>
        {
            e.ToTable("expense_categories").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.NameAr).HasColumnName("name_ar").HasMaxLength(100);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.Ignore(x => x.IsDeleted);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // ExpenseTemplate
        modelBuilder.Entity<ExpenseTemplate>(e =>
        {
            e.ToTable("expense_templates").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.CostCenter).HasColumnName("cost_center").HasMaxLength(30);
            e.Property(x => x.DefaultAmount).HasColumnName("default_amount").HasPrecision(12, 2);
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Ignore(x => x.UpdatedAt);
            e.Ignore(x => x.DeletedAt);
            e.Ignore(x => x.IsDeleted);
        });

        // Expense
        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("expenses").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ExpenseNumber).HasColumnName("expense_number").HasMaxLength(30).IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.CostCenter).HasColumnName("cost_center").HasMaxLength(50);
            e.Property(x => x.ExpenseDate).HasColumnName("expense_date");
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
            e.Property(x => x.Description).HasColumnName("description").IsRequired();
            e.Property(x => x.RelatedModule).HasColumnName("related_module").HasMaxLength(50);
            e.Property(x => x.RelatedEntityId).HasColumnName("related_entity_id");
            e.Property(x => x.VaultId).HasColumnName("vault_id");
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.AttachmentKey).HasColumnName("attachment_key").HasMaxLength(500);
            e.Property(x => x.AttachmentName).HasColumnName("attachment_name").HasMaxLength(200);
            e.Property(x => x.CreatedById).HasColumnName("created_by_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.Ignore(x => x.IsDeleted);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // AuditLogEntry — shared write model
        modelBuilder.Entity<AuditLogEntry>(e =>
        {
            e.ToTable("audit_log_entries").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(100);
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(100);
            e.Property(x => x.PerformedById).HasColumnName("performed_by_id");
            e.Property(x => x.Details).HasColumnName("details");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // VaultTransactionEntry — write model for vault_transactions
        modelBuilder.Entity<VaultTransactionEntry>(e =>
        {
            e.ToTable("vault_transactions").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.VaultId).HasColumnName("vault_id");
            e.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(30);
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
            e.Property(x => x.Direction).HasColumnName("direction").HasMaxLength(3);
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
