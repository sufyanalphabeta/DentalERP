using DentalERP.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Infrastructure;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemBarcode> ItemBarcodes => Set<ItemBarcode>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        // Soft delete global query filters
        modelBuilder.Entity<Item>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ItemCategory>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Warehouse>().HasQueryFilter(e => e.DeletedAt == null);
    }
}
