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

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",   // React Admin Dashboard
                "https://localhost:7134",  // Blazor CustomerPortal (ajusta se precisar)
                "http://localhost:5125"    // se teu CustomerPortal ou outro frontend usar HTTP
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.MapControllers();

app.Run();