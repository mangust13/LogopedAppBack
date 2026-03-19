using Microsoft.EntityFrameworkCore;
using UserService.Domain;

namespace UserService.Infrastructure;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<ChildAssignment> ChildAssignments => Set<ChildAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);

            u.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            u.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            u.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(50);

            u.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            u.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<ChildProfile>(cp =>
        {
            cp.HasKey(x => x.Id);

            cp.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            cp.Property(x => x.ProblemSounds)
                .HasMaxLength(500);

            cp.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            cp.HasOne(x => x.ParentUser)
                .WithMany()
                .HasForeignKey(x => x.ParentUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            cp.HasIndex(x => x.ParentUserId);
        });

        modelBuilder.Entity<ChildAssignment>(ca =>
        {
            ca.HasKey(x => x.Id);

            ca.Property(x => x.AssignedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            ca.HasOne(x => x.ChildProfile)
                .WithMany()
                .HasForeignKey(x => x.ChildProfileId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            ca.HasOne(x => x.Logoped)
                .WithMany()
                .HasForeignKey(x => x.LogopedUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            ca.HasIndex(x => new { x.ChildProfileId, x.LogopedUserId });
        });
    }
}