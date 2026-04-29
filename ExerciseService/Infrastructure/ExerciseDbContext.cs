using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Infrastructure;

public class ExerciseDbContext : DbContext
{
    public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) : base(options)
    {
    }

    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseTag> ExerciseTags { get; set; }
    public DbSet<ExerciseTagLink> ExerciseTagLinks { get; set; }
    public DbSet<Complex> Complexes { get; set; }
    public DbSet<ComplexItem> ComplexItems { get; set; }
    public DbSet<ComplexAssignment> ComplexAssignments { get; set; }
    public DbSet<SoundCard> SoundCards { get; set; }
    public DbSet<SoundPosition> SoundPositions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.VideoPath).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(100);
        });

        modelBuilder.Entity<ExerciseTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ExerciseTagLink>(entity =>
        {
            entity.HasKey(e => new { e.ExerciseId, e.TagId });

            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.Tags)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.Exercises)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Complex>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FolderName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);

            entity.HasIndex(e => e.LogopedId);
            entity.HasIndex(e => e.IsDefault);
        });

        modelBuilder.Entity<ComplexItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Complex)
                .WithMany(c => c.Exercises)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Exercise)
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ComplexId, e.ExerciseId });
        });

        modelBuilder.Entity<ComplexAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Complex)
                .WithMany(c => c.Assignments)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ComplexId, e.ChildId });
        });

        modelBuilder.Entity<SoundPosition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<SoundCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sound).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Word).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageFile).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.SoundCards)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.Sound, e.Word });
        });
    }
}