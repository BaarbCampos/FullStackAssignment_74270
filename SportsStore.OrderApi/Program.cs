using SportsStore.OrderApi.Configuration;
using SportsStore.OrderApi.Consumers;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔧 Configuração do RabbitMQ
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

// 🔥 GUARDA OS PEDIDOS (ESSENCIAL)
builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();

// 🔥 PUBLICA EVENTOS NO RABBITMQ
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

// 🔥 ESCUTA EVENTO FINAL (shipping-created)
builder.Services.AddHostedService<ShippingCreatedConsumer>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();