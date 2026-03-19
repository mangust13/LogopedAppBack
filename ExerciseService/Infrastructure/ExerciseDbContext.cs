using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Infrastructure;

public class ExerciseDbContext : DbContext
{
    public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) : base(options)
    {
    }

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseMainCategory> ExerciseMainCategories => Set<ExerciseMainCategory>();
    public DbSet<ExerciseTag> ExerciseTags => Set<ExerciseTag>();
    public DbSet<ExerciseTagLink> ExerciseTagLinks => Set<ExerciseTagLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Exercise
        modelBuilder.Entity<Exercise>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.VideoPath).IsRequired().HasMaxLength(500);
            e.Property(x => x.IconName).HasMaxLength(100);

            e.HasOne(x => x.MainCategory)
                .WithMany(x => x.Exercises)
                .HasForeignKey(x => x.MainCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ExerciseMainCategory
        modelBuilder.Entity<ExerciseMainCategory>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.Name).IsRequired().HasMaxLength(100);
            c.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            c.Property(x => x.FolderName).IsRequired().HasMaxLength(100);
            c.HasIndex(x => x.Name).IsUnique();
        });

        // ExerciseTag
        modelBuilder.Entity<ExerciseTag>(t =>
        {
            t.HasKey(x => x.Id);
            t.Property(x => x.Name).IsRequired().HasMaxLength(100);
            t.Property(x => x.Category).IsRequired().HasMaxLength(50);
            t.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            t.HasIndex(x => x.Name).IsUnique();
        });

        // ExerciseTagLink
        modelBuilder.Entity<ExerciseTagLink>(l =>
        {
            l.HasKey(x => new { x.ExerciseId, x.TagId });

            l.HasOne(x => x.Exercise)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            l.HasOne(x => x.Tag)
                .WithMany(x => x.Exercises)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}