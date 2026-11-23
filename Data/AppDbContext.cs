using EmpleadaHogar.Models;
using EmpleadaHogar.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmpleadaHogar.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserContext currentUser)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<EmployerProfile> Employers => Set<EmployerProfile>();
    public DbSet<EmployeeProfile> Employees => Set<EmployeeProfile>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<EconomicParameter> EconomicParameters => Set<EconomicParameter>();
    public DbSet<ContractTypeDefinition> ContractTypeDefinitions => Set<ContractTypeDefinition>();
    public DbSet<PayrollConceptDefinition> PayrollConceptDefinitions => Set<PayrollConceptDefinition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EmployerProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IdentificationNumber).HasMaxLength(24);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<EmployeeProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IdentificationNumber).HasMaxLength(24);
            entity.Property(x => x.PostalCode).HasMaxLength(5);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Province).HasMaxLength(100);
            entity.Property(x => x.WorkDays).HasMaxLength(20);
            entity.Property(x => x.SocialSecurityNumber).HasMaxLength(24);
            entity.Property(x => x.ContractType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.WeeklyHours).HasPrecision(5, 2);
            entity.Property(x => x.MonthlyBaseSalary).HasPrecision(12, 2);
            entity.Property(x => x.ExtraPaymentsMonthly).HasPrecision(12, 2);
            entity.Property(x => x.IrpfWithholdingPercent).HasPrecision(5, 2);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasOne(x => x.Employer).WithMany(x => x.Employees).HasForeignKey(x => x.EmployerProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<PayrollRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.EmployerNameSnapshot).HasMaxLength(300).IsRequired();
            entity.Property(x => x.EmployerIdentificationSnapshot).HasMaxLength(24);
            entity.Property(x => x.EmployerCccSnapshot).HasMaxLength(30);
            entity.Property(x => x.EmployerAddressSnapshot).HasMaxLength(250);
            entity.Property(x => x.EmployeeNameSnapshot).HasMaxLength(300).IsRequired();
            entity.Property(x => x.EmployeeIdentificationSnapshot).HasMaxLength(24);
            entity.Property(x => x.EmployeeSocialSecuritySnapshot).HasMaxLength(24);
            entity.Property(x => x.EmployeeEmailSnapshot).HasMaxLength(254);
            entity.Property(x => x.ContractTypeSnapshot).HasMaxLength(30).IsRequired();
            entity.Property(x => x.WeeklyHoursSnapshot).HasPrecision(5, 2);
            entity.Property(x => x.GrossPay).HasPrecision(12, 2);
            entity.Property(x => x.EmployeeSocialSecurity).HasPrecision(12, 2);
            entity.Property(x => x.IrpfWithholding).HasPrecision(12, 2);
            entity.Property(x => x.OtherEmployeeDeductions).HasPrecision(12, 2);
            entity.Property(x => x.EmployerSocialSecurity).HasPrecision(12, 2);
            entity.Property(x => x.NetPay).HasPrecision(12, 2);
            entity.Property(x => x.TotalEmployerCost).HasPrecision(12, 2);
            entity.HasIndex(x => new { x.EmployeeProfileId, x.Year, x.Month }).IsUnique();
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasOne(x => x.Employee).WithMany(x => x.PayrollRecords).HasForeignKey(x => x.EmployeeProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<PayrollLine>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(12, 2);
            entity.HasOne(x => x.Payroll).WithMany(x => x.Lines).HasForeignKey(x => x.PayrollRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<EconomicParameter>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.NumericValue).HasPrecision(14, 4);
            entity.Property(x => x.TextValue).HasMaxLength(1000);
            entity.Property(x => x.Unit).HasMaxLength(30);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.OwnerUserId, x.Code, x.ValidFrom }).IsUnique();
            entity.HasIndex(x => new { x.OwnerUserId, x.ValidFrom, x.ValidTo });
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<ContractTypeDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.OwnerUserId, x.Code }).IsUnique();
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });

        builder.Entity<PayrollConceptDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.DefaultValue).HasPrecision(14, 4);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.OwnerUserId, x.Code, x.ValidFrom }).IsUnique();
            entity.HasQueryFilter(x => x.OwnerUserId == currentUser.UserId);
        });
    }
}




