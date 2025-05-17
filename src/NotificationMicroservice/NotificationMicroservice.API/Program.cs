using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NotificationMicroservice.Application.EventHandlers;
using NotificationMicroservice.Application.Services;
using NotificationMicroservice.Domain.Repositories;
using NotificationMicroservice.Infrastructure.Data;
using NotificationMicroservice.Infrastructure.Repositories;
using NotificationMicroservice.Infrastructure.Services;
using Shared.Contracts.Events;

// Устанавливаем инвариантную культуру
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Add Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add HttpClient for OrderService
builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService:Url"] ?? "http://localhost:5131");
});

// Add Event Handlers
builder.Services.AddScoped<OrderEventHandler>();
builder.Services.AddScoped<PaymentEventHandler>();
builder.Services.AddScoped<DeliveryEventHandler>();

// Add RabbitMQ Service
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Microservice API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<NotificationDbContext>();
    context.Database.Migrate();
}

app.Run();

