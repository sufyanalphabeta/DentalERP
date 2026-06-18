using DentalERP.Modules.Radiology.Domain.Entities;
using DentalERP.Modules.Radiology.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Infrastructure;

public sealed class RadiologyDbContext(DbContextOptions<RadiologyDbContext> options) : DbContext(options)
{
    public DbSet<RadiologyType> RadiologyTypes => Set<RadiologyType>();
    public DbSet<RadiologyOrder> RadiologyOrders => Set<RadiologyOrder>();
    public DbSet<RadiologyImage> RadiologyImages => Set<RadiologyImage>();
    public DbSet<RadiologyReport> RadiologyReports => Set<RadiologyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RadiologyTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyOrderConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyImageConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyReportConfiguration());
    }
}
