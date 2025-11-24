using Microsoft.EntityFrameworkCore;
using ProgressService.Infrastructure;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;

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

        // OpenTelemetry
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService("ProgressService"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddEntityFrameworkCoreInstrumentation();
                tracing.AddZipkinExporter(exporter =>
                {
                    exporter.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                });
            });

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
