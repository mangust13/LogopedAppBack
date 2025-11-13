using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        // Створюємо WebApplicationBuilder
        var builder = WebApplication.CreateBuilder(args);

        // Додаємо Serilog
        builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration));

        // Реєструємо конфіг секцію RabbitMQ
        builder.Services.Configure<RabbitOptions>(
            builder.Configuration.GetSection("RabbitMQ"));

        // Додаємо HTTP Client для ProgressService
        builder.Services.AddHttpClient("ProgressService", client =>
        {
            var baseUrl = builder.Configuration["Services:ProgressServiceUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
                client.BaseAddress = new Uri(baseUrl);
        });

        // Інші стандартні сервіси
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Swagger тільки в Dev
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();

        Task.Run(() => ResultListener.StartAsync());
        app.Run();
    }
}

// ===== DTO клас для RabbitMQ config =====
public class RabbitOptions
{
    public string Host { get; set; }
    public string Exchange { get; set; }
    public string AudioRoutingKey { get; set; }
    public string ResultRoutingKey { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
