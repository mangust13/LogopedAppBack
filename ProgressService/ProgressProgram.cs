using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProgressService.Infrastructure;
using ProgressService.Services;
using System.Text;

namespace ProgressService;

public class ProgressProgram
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "5003";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        builder.Services.AddDbContext<ProgressDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IActivityService, ActivityService>();
        builder.Services.AddScoped<IGameProgressService, GameProgressService>();
        builder.Services.AddScoped<ILogopedSessionService, LogopedSessionService>();

        var jwtSection = builder.Configuration.GetSection("Jwt");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"]!))
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "ProgressService", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new()
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Введіть: Bearer {token}"
            });
            options.AddSecurityRequirement(new()
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            await db.Database.MigrateAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ProgressService" }));
        app.MapControllers();

        await app.RunAsync();
    }
}