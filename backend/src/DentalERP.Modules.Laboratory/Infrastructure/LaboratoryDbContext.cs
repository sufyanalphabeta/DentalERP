using DentalERP.Modules.Laboratory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Infrastructure;

public sealed class LaboratoryDbContext(DbContextOptions<LaboratoryDbContext> options) : DbContext(options)
{
    public DbSet<ExternalLab> ExternalLabs => Set<ExternalLab>();
    public DbSet<LabClient> LabClients => Set<LabClient>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabOrderItem> LabOrderItems => Set<LabOrderItem>();
    public DbSet<LabResult> LabResults => Set<LabResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(LaboratoryDbContext).Assembly);
}
