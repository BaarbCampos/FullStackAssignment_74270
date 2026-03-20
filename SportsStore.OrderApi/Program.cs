using SportsStore.OrderApi.Configuration;
using SportsStore.OrderApi.Messaging;
using SportsStore.OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();