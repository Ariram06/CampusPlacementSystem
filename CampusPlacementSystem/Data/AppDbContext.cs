using CampusPlacementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<College> Colleges => Set<College>();
    public DbSet<PlacementOfficer> PlacementOfficers => Set<PlacementOfficer>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyHR> CompanyHRs => Set<CompanyHR>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<JobOpening> JobOpenings => Set<JobOpening>();
    public DbSet<JobEligibleDepartment> JobEligibleDepartments => Set<JobEligibleDepartment>();
    public DbSet<JobEligibleGraduationYear> JobEligibleGraduationYears => Set<JobEligibleGraduationYear>();
    public DbSet<JobTargetCollege> JobTargetColleges => Set<JobTargetCollege>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<RecruitmentDrive> RecruitmentDrives => Set<RecruitmentDrive>();
    public DbSet<InterviewRound> InterviewRounds => Set<InterviewRound>();
    public DbSet<InterviewSchedule> InterviewSchedules => Set<InterviewSchedule>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. College
        builder.Entity<College>(entity =>
        {
            entity.HasIndex(c => c.CollegeCode)
                  .IsUnique();

            entity.Property(c => c.CollegeCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(c => c.CollegeName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(c => c.OfficialEmail)
                  .IsRequired()
                  .HasMaxLength(150);
        });

        // 2. PlacementOfficer
        builder.Entity<PlacementOfficer>(entity =>
        {
            entity.HasIndex(po => po.UserId)
                  .IsUnique();

            entity.HasOne(po => po.User)
                  .WithOne(u => u.PlacementOfficer)
                  .HasForeignKey<PlacementOfficer>(po => po.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(po => po.College)
                  .WithMany(c => c.PlacementOfficers)
                  .HasForeignKey(po => po.CollegeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Company
        builder.Entity<Company>(entity =>
        {
            entity.Property(c => c.CompanyName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(c => c.CompanyEmail)
                  .IsRequired()
                  .HasMaxLength(150);
        });

        // 4. CompanyHR
        builder.Entity<CompanyHR>(entity =>
        {
            entity.HasIndex(hr => hr.UserId)
                  .IsUnique();

            entity.HasOne(hr => hr.User)
                  .WithOne(u => u.CompanyHR)
                  .HasForeignKey<CompanyHR>(hr => hr.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(hr => hr.Company)
                  .WithMany(c => c.HRUsers)
                  .HasForeignKey(hr => hr.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. Student
        builder.Entity<Student>(entity =>
        {
            entity.HasIndex(s => s.UserId)
                  .IsUnique();

            // Unique composite index: RegisterNumber must be unique within a college
            entity.HasIndex(s => new { s.CollegeId, s.RegisterNumber })
                  .IsUnique();

            entity.Property(s => s.CGPA)
                  .HasPrecision(3, 2);

            entity.HasOne(s => s.User)
                  .WithOne(u => u.Student)
                  .HasForeignKey<Student>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.College)
                  .WithMany(c => c.Students)
                  .HasForeignKey(s => s.CollegeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.EnrolledByOfficer)
                  .WithMany(po => po.EnrolledStudents)
                  .HasForeignKey(s => s.PlacementOfficerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 6. JobOpening
        builder.Entity<JobOpening>(entity =>
        {
            entity.Property(j => j.SalaryPackage)
                  .HasPrecision(18, 2);

            entity.Property(j => j.MinimumCGPA)
                  .HasPrecision(3, 2);

            entity.HasOne(j => j.Company)
                  .WithMany(c => c.JobOpenings)
                  .HasForeignKey(j => j.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.CreatedByHR)
                  .WithMany(hr => hr.CreatedJobOpenings)
                  .HasForeignKey(j => j.CreatedByHRId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(j => j.EligibleDepartments)
                  .WithOne(d => d.JobOpening)
                  .HasForeignKey(d => d.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(j => j.EligibleGraduationYears)
                  .WithOne(y => y.JobOpening)
                  .HasForeignKey(y => y.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. JobTargetCollege
        builder.Entity<JobTargetCollege>(entity =>
        {
            entity.HasIndex(tc => new { tc.JobId, tc.CollegeId })
                  .IsUnique();

            entity.HasOne(tc => tc.JobOpening)
                  .WithMany(j => j.TargetColleges)
                  .HasForeignKey(tc => tc.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tc => tc.College)
                  .WithMany(c => c.TargetColleges)
                  .HasForeignKey(tc => tc.CollegeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 8. JobApplication
        builder.Entity<JobApplication>(entity =>
        {
            // Unique composite index: Student can apply to a job only once
            entity.HasIndex(a => new { a.StudentId, a.JobId })
                  .IsUnique();

            entity.HasOne(a => a.JobOpening)
                  .WithMany(j => j.Applications)
                  .HasForeignKey(a => a.JobId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Student)
                  .WithMany(s => s.JobApplications)
                  .HasForeignKey(a => a.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 9. RecruitmentDrive
        builder.Entity<RecruitmentDrive>(entity =>
        {
            entity.HasOne(rd => rd.JobOpening)
                  .WithMany(j => j.RecruitmentDrives)
                  .HasForeignKey(rd => rd.JobId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rd => rd.College)
                  .WithMany(c => c.RecruitmentDrives)
                  .HasForeignKey(rd => rd.CollegeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 10. InterviewRound
        builder.Entity<InterviewRound>(entity =>
        {
            entity.HasIndex(ir => new { ir.JobId, ir.RoundNumber })
                  .IsUnique();

            entity.HasOne(ir => ir.JobOpening)
                  .WithMany(j => j.InterviewRounds)
                  .HasForeignKey(ir => ir.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 11. InterviewSchedule
        builder.Entity<InterviewSchedule>(entity =>
        {
            entity.HasOne(s => s.Application)
                  .WithMany(a => a.InterviewSchedules)
                  .HasForeignKey(s => s.ApplicationId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.InterviewRound)
                  .WithMany(r => r.InterviewSchedules)
                  .HasForeignKey(s => s.InterviewRoundId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 12. ApplicationStatusHistory
        builder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.HasOne(h => h.Application)
                  .WithMany(a => a.StatusHistories)
                  .HasForeignKey(h => h.ApplicationId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(h => h.UpdatedByUser)
                  .WithMany(u => u.UpdatedStatusHistories)
                  .HasForeignKey(h => h.UpdatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 13. RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.TokenHash)
                  .IsUnique();

            entity.HasIndex(rt => rt.UserId);

            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
