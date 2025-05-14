using Microsoft.EntityFrameworkCore;
using OrderMicroservice.API.Extensions;
using OrderMicroservice.Application.Interfaces;
using OrderMicroservice.Contracts.Messages;
using OrderMicroservice.Infrastructure.Data;
using OrderMicroservice.Infrastructure.Services;
using OrderMicroservice.Infrastructure.Settings;
using Shared.Contracts;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOrderServices(builder.Configuration);

// Configure RabbitMQ settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Register payment event handlers
builder.Services.AddScoped<IMessageHandler<PaymentCompletedEvent>, PaymentCompletedHandler>();
builder.Services.AddScoped<IMessageHandler<PaymentFailedEvent>, PaymentFailedHandler>();
builder.Services.AddScoped<IMessageHandler<PaymentRefundedEvent>, PaymentRefundedHandler>();
builder.Services.AddScoped<IMessageHandler<DeliveryStatusUpdatedEvent>, DeliveryStatusChangedHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Initialize message handler
    services.GetRequiredService<CartCheckoutMessageHandler>();
}

// Subscribe to payment events
using (var scope = app.Services.CreateScope())
{
    var messageBroker = scope.ServiceProvider.GetRequiredService<IMessageBroker>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Setting up payment events subscription...");
    await messageBroker.SubscribeToPaymentEventsAsync();
    logger.LogInformation("Successfully subscribed to payment events");

    logger.LogInformation("Setting up delivery events subscription...");
    await messageBroker.SubscribeToDeliveryEventsAsync();
    logger.LogInformation("Successfully subscribed to delivery events");
}

app.Run();

