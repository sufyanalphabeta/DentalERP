using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Infrastructure;

// Lightweight read models for cross-module name lookups (read-only, no tracking)
public sealed record PatientLookup(Guid Id, string FullName);
public sealed record UserLookup(Guid Id, string FullName);
public sealed record InstallmentPlanLookup(Guid Id, string InvoiceNumber, string PatientName);

public sealed class FinancialDbContext(DbContextOptions<FinancialDbContext> options) : DbContext(options)
{
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<Vault> Vaults => Set<Vault>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<VaultTransaction> VaultTransactions => Set<VaultTransaction>();
    public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
    public DbSet<InstallmentPayment> InstallmentPayments => Set<InstallmentPayment>();
    public DbSet<AdvancePayment> AdvancePayments => Set<AdvancePayment>();
    public DbSet<CommissionRecord> CommissionRecords => Set<CommissionRecord>();
    public DbSet<InsuranceCompany> InsuranceCompanies => Set<InsuranceCompany>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<InsurancePayment> InsurancePayments => Set<InsurancePayment>();
    public DbSet<VaultTransfer> VaultTransfers => Set<VaultTransfer>();

    // Cross-module read-only lookups — aliases match C# property names
    public IQueryable<PatientLookup> PatientNames =>
        Database.SqlQuery<PatientLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM patients WHERE deleted_at IS NULL");

    public IQueryable<UserLookup> UserNames =>
        Database.SqlQuery<UserLookup>($"SELECT id AS \"Id\", full_name AS \"FullName\" FROM users");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
}
