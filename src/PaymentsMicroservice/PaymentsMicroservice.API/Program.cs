using Microsoft.EntityFrameworkCore;
using PaymentsMicroservice.Application.Interfaces;
using PaymentsMicroservice.Infrastructure.Data;
using PaymentsMicroservice.Infrastructure.Services;
using Shared.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();
builder.Services.AddScoped<IMessageHandler<OrderCreatedEvent>, OrderCreatedHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Subscribe to messages
var messageBroker = app.Services.GetRequiredService<IMessageBroker>();

// Create a scope for subscription
using (var scope = app.Services.CreateScope())
{
    var orderCreatedHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<OrderCreatedEvent>>();
    await messageBroker.SubscribeAsync("payments.order.created", "order.created", orderCreatedHandler);
}

app.Run();
