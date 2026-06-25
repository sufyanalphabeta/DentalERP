using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Infrastructure;

internal sealed class AssetsDbContext : DbContext
{
    public AssetsDbContext(DbContextOptions<AssetsDbContext> options) : base(options) { }

    internal DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    internal DbSet<Asset> Assets => Set<Asset>();
    internal DbSet<AssetDocument> AssetDocuments => Set<AssetDocument>();
    internal DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();
    internal DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AssetCategory
        modelBuilder.Entity<AssetCategory>(e =>
        {
            e.ToTable("asset_categories").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.NameAr).HasColumnName("name_ar").HasMaxLength(100);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.DepreciationRate).HasColumnName("depreciation_rate").HasPrecision(5, 2);
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.Ignore(x => x.IsDeleted);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // Asset
        modelBuilder.Entity<Asset>(e =>
        {
            e.ToTable("assets").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AssetTag).HasColumnName("asset_tag").HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.AssetTag).IsUnique();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.PurchaseDate).HasColumnName("purchase_date");
            e.Property(x => x.PurchaseCost).HasColumnName("purchase_cost").HasPrecision(12, 2);
            e.Property(x => x.Location).HasColumnName("location").HasMaxLength(200);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100);
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedById).HasColumnName("created_by_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            e.Ignore(x => x.IsDeleted);
            e.HasQueryFilter(x => x.DeletedAt == null);
        });

        // AssetDocument — column names match migration 027
        modelBuilder.Entity<AssetDocument>(e =>
        {
            e.ToTable("asset_documents").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AssetId).HasColumnName("asset_id");
            e.Property(x => x.DocumentType).HasColumnName("document_type").HasMaxLength(50);
            e.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(200);
            e.Property(x => x.FileKey).HasColumnName("file_key").HasMaxLength(500);
            e.Property(x => x.FileSize).HasColumnName("file_size");
            e.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(100);
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.UploadedById).HasColumnName("uploaded_by_id");
            // migration uses uploaded_at — map CreatedAt to it
            e.Property(x => x.CreatedAt).HasColumnName("uploaded_at");
            e.Ignore(x => x.UpdatedAt);
            e.Ignore(x => x.DeletedAt);
            e.Ignore(x => x.IsDeleted);
        });

        // AssetMaintenance — column names match migration 027
        modelBuilder.Entity<AssetMaintenance>(e =>
        {
            e.ToTable("asset_maintenance").HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.AssetId).HasColumnName("asset_id");
            e.Property(x => x.MaintenanceDate).HasColumnName("maintenance_date");
            e.Property(x => x.Cost).HasColumnName("cost").HasPrecision(12, 2);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Vendor).HasColumnName("vendor").HasMaxLength(200);
            e.Property(x => x.NextMaintenanceDate).HasColumnName("next_maintenance_date");
            e.Property(x => x.ExpenseId).HasColumnName("expense_id");
            // migration has performed_by_id
            e.Property(x => x.CreatedById).HasColumnName("performed_by_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Ignore(x => x.DeletedAt);
            e.Ignore(x => x.IsDeleted);
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
    }
}
