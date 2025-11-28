using Microsoft.EntityFrameworkCore;
using ProgressService.Infrastructure;

namespace ProgressService;

public class ProgressService
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Database
        builder.Services.AddDbContext<ProgressDbContext>(opt =>
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default"))
        );

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Apply migrations
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/health", () =>
            Results.Ok(new { status = "Ok", service = "ProgressService" })
        );

        app.MapControllers();

        await app.RunAsync();
    }
}
