using SportsStore.OrderApi.Consumers;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

builder.Services.AddHostedService<InventoryConfirmedConsumer>();
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