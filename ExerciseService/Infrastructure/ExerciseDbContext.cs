using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ExerciseService.Infrastructure;

public class ExerciseDbContext : DbContext
{
    public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options)
        : base(options) { }

    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseComplex> Complexes { get; set; }
    public DbSet<ComplexItem> ComplexItems { get; set; }
    public DbSet<ChildHomework> Homeworks { get; set; }
}
