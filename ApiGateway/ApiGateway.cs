using System.Text;
using Yarp.ReverseProxy;

namespace ApiGateway;
public class ApiGateway
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "ProgressService",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new()
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "¬вед≥ть: Bearer {token}"
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


        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.MapGet("/", () => new { gateway = "Logoped API Gateway", status = "ok" });
        app.MapReverseProxy();

        await app.RunAsync();
    }
}
