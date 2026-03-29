using Microsoft.EntityFrameworkCore;
using SportsStore.OrderApi.Configuration;
using SportsStore.OrderApi.Consumers;
using SportsStore.OrderApi.Data;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderService, EfOrderService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

builder.Services.AddHostedService<InventoryConfirmedConsumer>();
builder.Services.AddHostedService<InventoryFailedConsumer>();
builder.Services.AddHostedService<PaymentApprovedConsumer>();
builder.Services.AddHostedService<PaymentRejectedConsumer>();
builder.Services.AddHostedService<ShippingCreatedConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();