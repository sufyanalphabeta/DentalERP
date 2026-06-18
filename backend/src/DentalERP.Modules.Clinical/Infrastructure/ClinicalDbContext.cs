using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Infrastructure;

public sealed class ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : DbContext(options)
{
    public DbSet<Tooth> Teeth => Set<Tooth>();
    public DbSet<DentalChartEntry> DentalChartEntries => Set<DentalChartEntry>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanItem> TreatmentPlanItems => Set<TreatmentPlanItem>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<PatientMedia> PatientMedia => Set<PatientMedia>();
    public DbSet<DoctorAssignment> DoctorAssignments => Set<DoctorAssignment>();
    public DbSet<PatientTimelineEvent> PatientTimeline => Set<PatientTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalDbContext).Assembly);
    }
}
