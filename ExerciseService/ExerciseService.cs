using ExerciseService.Infrastructure;
using ExerciseService.Messaging;
using ExerciseService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace ExerciseService;

public class ExerciseService
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration));

        builder.Services.Configure<RabbitOptions>(
            builder.Configuration.GetSection("RabbitMQ"));

        builder.Services.AddDbContext<ExerciseDbContext>(opt =>
            opt.UseSqlite("Data Source=exercise.db"));

        builder.Services.AddHttpClient("ProgressService", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:ProgressServiceUrl"]!);
        });

        // 2 варіант для лаби
        builder.Services.AddHttpClient("SpeechAI", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:SpeechAIUrl"]!);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r =>
                r.AddService("ExerciseService"))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation();
                t.AddSource("ExerciseService.RabbitMQ");
                t.AddZipkinExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                });
            });

        builder.Services.AddSingleton<ProgressReporter>();
        builder.Services.AddSingleton<RabbitMqPublisher>();
        builder.Services.AddHostedService<ResultListener>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        //using (var scope = app.Services.CreateScope())
        //{
        //    var db = scope.ServiceProvider.GetRequiredService<ExerciseDbContext>();
        //    db.Database.Migrate();
        //}

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ExerciseService" }));
        app.MapControllers();
        await app.RunAsync();
    }
}
