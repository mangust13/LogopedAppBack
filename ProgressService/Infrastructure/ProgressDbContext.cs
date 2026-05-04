using Microsoft.EntityFrameworkCore;
using ProgressService.Domain;

namespace ProgressService.Infrastructure;

public class ProgressDbContext : DbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

    public DbSet<GameProgress> GameProgresses => Set<GameProgress>();
    public DbSet<LogopedSession> LogopedSessions => Set<LogopedSession>();
    public DbSet<DailyActivity> DailyActivities => Set<DailyActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Sound).HasMaxLength(10);
            e.Property(x => x.GameType).HasMaxLength(50);
            e.HasIndex(x => new { x.ChildId, x.Sound, x.PositionCode, x.GameType }).IsUnique();
        });

        modelBuilder.Entity<LogopedSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.Property(x => x.SoundsWorkedOn).HasMaxLength(200);
            e.HasIndex(x => x.ChildId);
            e.HasIndex(x => x.LogopedId);
        });

        modelBuilder.Entity<DailyActivity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ActivityType).HasMaxLength(50);
            e.HasIndex(x => new { x.ChildId, x.Date, x.ActivityType }).IsUnique();
        });
    }
}