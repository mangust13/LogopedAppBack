using Microsoft.EntityFrameworkCore;
using UserService.Domain;

namespace UserService.Infrastructure;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<ChildAssignment> ChildAssignments => Set<ChildAssignment>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();

        modelBuilder.Entity<ChildProfile>()
            .HasOne(c => c.ParentUser)
            .WithMany()
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChildAssignment>()
            .HasOne(x => x.ChildProfile)
            .WithMany()
            .HasForeignKey(x => x.ChildProfileId);

        modelBuilder.Entity<ChildAssignment>()
            .HasOne(x => x.Logoped)
            .WithMany()
            .HasForeignKey(x => x.LogopedUserId);
    }
}