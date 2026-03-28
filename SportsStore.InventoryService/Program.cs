using SportsStore.InventoryService;
using SportsStore.InventoryService.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqSettings>>().Value);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();