using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ExerciseService.Infrastructure;

public class ExerciseDbContext : DbContext
{
    public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options)
        : base(options) { }

    public DbSet<Exercise> Exercises => Set<Exercise>();
}
