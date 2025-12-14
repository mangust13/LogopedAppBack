using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProgressService.Infrastructure;
using System.Text;

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

        // JWT config
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () =>
            Results.Ok(new { status = "Ok", service = "ProgressService" })
        );

        app.MapControllers();

        await app.RunAsync();
    }
}
