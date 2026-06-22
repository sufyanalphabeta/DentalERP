using DentalERP.Modules.Laboratory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Infrastructure;

public sealed record PatientLookup(Guid Id, string FullName);
public sealed record UserLookup(Guid Id, string FullName);

public sealed class LaboratoryDbContext(DbContextOptions<LaboratoryDbContext> options) : DbContext(options)
{
    public DbSet<ExternalLab> ExternalLabs => Set<ExternalLab>();
    public DbSet<LabClient> LabClients => Set<LabClient>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabOrderItem> LabOrderItems => Set<LabOrderItem>();
    public DbSet<LabResult> LabResults => Set<LabResult>();

    public IQueryable<PatientLookup> PatientNames =>
        Database.SqlQuery<PatientLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM patients WHERE deleted_at IS NULL");

    public IQueryable<UserLookup> UserNames =>
        Database.SqlQuery<UserLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM users");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaboratoryDbContext).Assembly);
}
