using ExerciseService.Infrastructure;
using ExerciseService.Infrastructure.Seed;
using ExerciseService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ExerciseService;

public class ExerciseProgram
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Services
        builder.Services.AddDbContext<ExerciseDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpContextAccessor();
        var userServiceUrl = builder.Configuration["Services:UserService"];

        builder.Services.AddHttpClient<IUserService, UserService>(client =>
        {
            if (string.IsNullOrWhiteSpace(userServiceUrl))
            {
                throw new Exception("UserService URL is not configured");
            }

            client.BaseAddress = new Uri(userServiceUrl);
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "ExerciseProgram", Version = "v1" });

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

        // JWT
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
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();

        // Apply migration + seed
        bool forceReseed = false;

        if (forceReseed)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ExerciseDbContext>();
            var filePath = Path.Combine(env.WebRootPath, "static", "preparation", "exercises.xlsx");

            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            await ExerciseSeeder.SeedAsync(db, filePath);
            await SoundCardSeeder.SeedAsync(db);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ExerciseProgram" }));
        app.MapControllers();

        await app.RunAsync();
    }
}