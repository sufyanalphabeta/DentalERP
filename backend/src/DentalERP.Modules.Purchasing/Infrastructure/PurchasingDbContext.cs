using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Domain.Internal;
using DentalERP.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Infrastructure;

public sealed class PurchasingDbContext(DbContextOptions<PurchasingDbContext> options) : DbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierItem> SupplierItems => Set<SupplierItem>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

    // Read-only views of Inventory tables (no migration ownership)
    public DbSet<Item> Items => Set<Item>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    // Write model for vault_transactions (owned by Financial, no migration here)
    internal DbSet<VaultTransactionEntry> VaultTransactions => Set<VaultTransactionEntry>();

    // Write model for audit_log_entries (business events — separate from IAM audit_logs)
    internal DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurchasingDbContext).Assembly);

        // Soft delete filters
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<PurchaseInvoice>().HasQueryFilter(e => e.DeletedAt == null);

        // Inventory tables — mapped read-only (no migration ownership)
        modelBuilder.Entity<Item>().ToTable("items").HasNoKey()
            .Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Item>().Property(x => x.ItemCode).HasColumnName("item_code");
        modelBuilder.Entity<Item>().Property(x => x.Name).HasColumnName("name");
        modelBuilder.Entity<Item>().Property(x => x.UnitCost).HasColumnName("unit_cost");
        modelBuilder.Entity<Item>().Property(x => x.SalePrice).HasColumnName("sale_price");
        modelBuilder.Entity<Item>().Property(x => x.AllowNegativeStock).HasColumnName("allow_negative_stock");
        modelBuilder.Entity<Item>().Property(x => x.Barcode).HasColumnName("barcode");
        modelBuilder.Entity<Item>().Property(x => x.IsActive).HasColumnName("is_active");
        modelBuilder.Entity<Item>().Property(x => x.UnitOfMeasureId).HasColumnName("unit_of_measure_id");
        modelBuilder.Entity<Item>().Property(x => x.CategoryId).HasColumnName("category_id");
        modelBuilder.Entity<Item>().Property(x => x.NameAr).HasColumnName("name_ar");
        modelBuilder.Entity<Item>().Property(x => x.ReorderLevel).HasColumnName("reorder_level");
        modelBuilder.Entity<Item>().Property(x => x.ReorderQuantity).HasColumnName("reorder_quantity");
        modelBuilder.Entity<Item>().Property(x => x.IsExpiryTracked).HasColumnName("is_expiry_tracked");
        modelBuilder.Entity<Item>().Property(x => x.StorageConditions).HasColumnName("storage_conditions");
        modelBuilder.Entity<Item>().Property(x => x.Notes).HasColumnName("notes");
        modelBuilder.Entity<Item>().Property(x => x.UpdatedAt).HasColumnName("updated_at");
        modelBuilder.Entity<Item>().Property(x => x.DeletedAt).HasColumnName("deleted_at");
        modelBuilder.Entity<Item>().Property(x => x.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<Item>().Ignore(x => x.DomainEvents).Ignore(x => x.Barcodes);

        modelBuilder.Entity<StockBatch>().ToTable("stock_batches").HasKey(x => x.Id);
        modelBuilder.Entity<StockBatch>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<StockBatch>().Property(x => x.ItemId).HasColumnName("item_id");
        modelBuilder.Entity<StockBatch>().Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        modelBuilder.Entity<StockBatch>().Property(x => x.Quantity).HasColumnName("quantity");
        modelBuilder.Entity<StockBatch>().Property(x => x.UnitCost).HasColumnName("unit_cost");
        modelBuilder.Entity<StockBatch>().Property(x => x.IsDepleted).HasColumnName("is_depleted");
        modelBuilder.Entity<StockBatch>().Property(x => x.BatchNumber).HasColumnName("batch_number");
        modelBuilder.Entity<StockBatch>().Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        modelBuilder.Entity<StockBatch>().Property(x => x.ReceivedDate).HasColumnName("received_date");
        modelBuilder.Entity<StockBatch>().Property(x => x.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<StockMovement>().ToTable("stock_movements").HasKey(x => x.Id);
        modelBuilder.Entity<StockMovement>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<StockMovement>().Property(x => x.MovementNumber).HasColumnName("movement_number");
        modelBuilder.Entity<StockMovement>().Property(x => x.ItemId).HasColumnName("item_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.BatchId).HasColumnName("batch_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.MovementType).HasColumnName("movement_type");
        modelBuilder.Entity<StockMovement>().Property(x => x.Direction).HasColumnName("direction");
        modelBuilder.Entity<StockMovement>().Property(x => x.Quantity).HasColumnName("quantity");
        modelBuilder.Entity<StockMovement>().Property(x => x.UnitCost).HasColumnName("unit_cost");
        modelBuilder.Entity<StockMovement>().Property(x => x.TotalCost).HasColumnName("total_cost");
        modelBuilder.Entity<StockMovement>().Property(x => x.DestinationType).HasColumnName("destination_type");
        modelBuilder.Entity<StockMovement>().Property(x => x.DestinationId).HasColumnName("destination_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.ReferenceId).HasColumnName("reference_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.ReferenceType).HasColumnName("reference_type");
        modelBuilder.Entity<StockMovement>().Property(x => x.IsNegativeStock).HasColumnName("is_negative_stock");
        modelBuilder.Entity<StockMovement>().Property(x => x.Notes).HasColumnName("notes");
        modelBuilder.Entity<StockMovement>().Property(x => x.CreatedById).HasColumnName("created_by_id");
        modelBuilder.Entity<StockMovement>().Property(x => x.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<Warehouse>().ToTable("warehouses").HasKey(x => x.Id);
        modelBuilder.Entity<Warehouse>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<Warehouse>().Property(x => x.Name).HasColumnName("name");
        modelBuilder.Entity<Warehouse>().Property(x => x.DeletedAt).HasColumnName("deleted_at");
        modelBuilder.Entity<Warehouse>().Property(x => x.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<Warehouse>().Ignore(x => x.DomainEvents);

        // AuditLogEntry — write model for audit_logs (migration 026)
        modelBuilder.Entity<AuditLogEntry>().ToTable("audit_log_entries").HasKey(x => x.Id);
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.EntityType).HasColumnName("entity_type");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.EntityId).HasColumnName("entity_id");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.Action).HasColumnName("action");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.PerformedById).HasColumnName("performed_by_id");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.Details).HasColumnName("details");
        modelBuilder.Entity<AuditLogEntry>().Property(x => x.CreatedAt).HasColumnName("created_at");

        // VaultTransactionEntry — write model for vault_transactions (Financial owns migration)
        modelBuilder.Entity<VaultTransactionEntry>().ToTable("vault_transactions").HasKey(x => x.Id);
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.Id).HasColumnName("id");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.VaultId).HasColumnName("vault_id");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.TransactionType).HasColumnName("transaction_type");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.Direction).HasColumnName("direction");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.Notes).HasColumnName("notes");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
        modelBuilder.Entity<VaultTransactionEntry>().Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
