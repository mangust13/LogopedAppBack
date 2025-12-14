using System.Text;
using Yarp.ReverseProxy;

namespace ApiGateway;
public class ApiGateway
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // YARP
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        app.UseCors();
        app.MapGet("/", () => new { gateway = "Logoped API Gateway", status = "ok" });

        // Proxy routes
        app.MapReverseProxy();

        await app.RunAsync();
    }
}
