using Microsoft.EntityFrameworkCore;
using UserService.Domain;

namespace UserService.Infrastructure;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
    }
}