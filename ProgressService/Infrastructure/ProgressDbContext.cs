using Microsoft.EntityFrameworkCore;
using ProgressService.Domain;

namespace ProgressService.Infrastructure;

public class ProgressDbContext : DbContext
{
    public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }

    public DbSet<ProgressRecord> Records => Set<ProgressRecord>();
}
