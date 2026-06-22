using DentalERP.Modules.Radiology.Domain.Entities;
using DentalERP.Modules.Radiology.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Infrastructure;

public sealed record PatientLookup(Guid Id, string FullName);
public sealed record UserLookup(Guid Id, string FullName);

public sealed class RadiologyDbContext(DbContextOptions<RadiologyDbContext> options) : DbContext(options)
{
    public DbSet<RadiologyType> RadiologyTypes => Set<RadiologyType>();
    public DbSet<RadiologyOrder> RadiologyOrders => Set<RadiologyOrder>();
    public DbSet<RadiologyImage> RadiologyImages => Set<RadiologyImage>();
    public DbSet<RadiologyReport> RadiologyReports => Set<RadiologyReport>();

    public IQueryable<PatientLookup> PatientNames =>
        Database.SqlQuery<PatientLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM patients WHERE deleted_at IS NULL");

    public IQueryable<UserLookup> UserNames =>
        Database.SqlQuery<UserLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM users");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RadiologyTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyOrderConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyImageConfiguration());
        modelBuilder.ApplyConfiguration(new RadiologyReportConfiguration());
    }
}
