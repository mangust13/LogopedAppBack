using ExerciseService.Infrastructure;
using ExerciseService.Messaging;
using ExerciseService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ExerciseService;

public class ExerciseService
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<RabbitOptions>(
            builder.Configuration.GetSection("RabbitMQ"));

        builder.Services.AddDbContext<ExerciseDbContext>(opt =>
            opt.UseSqlite("Data Source=exercise.db"));

        builder.Services.AddHttpClient("ProgressService", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:ProgressServiceUrl"]!);
        });

        builder.Services.AddSingleton<ProgressReporter>();
        builder.Services.AddSingleton<RabbitMqPublisher>();
        builder.Services.AddHostedService<ResultListener>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // JWT конфігурація
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Якщо потрібна міграція бази, розкоментуй
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ExerciseService" }));
        app.MapControllers();

        await app.RunAsync();
    }
}
