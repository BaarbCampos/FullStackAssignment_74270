using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SportsStore.Models;
using SportsStore.Services.Payments;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// SERILOG (ANTES DO BUILD)
// --------------------
builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext()
);

// --------------------
// SERVICES
// --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DbContexts
builder.Services.AddDbContext<StoreDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("SportsStoreConnection"));
});

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});

// Repositories
builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

// Stripe Payment Service
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

// Session + Cart
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();

// --------------------
// BUILD APP
// --------------------
var app = builder.Build();

// --------------------
// STARTUP LOG
// --------------------
app.Logger.LogInformation("Application starting. Environment: {Env}", app.Environment.EnvironmentName);

// --------------------
// STRIPE CONFIGURATION
// --------------------
var stripeKey = app.Configuration["Stripe:SecretKey"];

if (string.IsNullOrWhiteSpace(stripeKey))
{
    app.Logger.LogWarning("Stripe SecretKey is NOT configured. Payments will fail until Stripe:SecretKey is set.");
}
else
{
    StripeConfiguration.ApiKey = stripeKey;
    app.Logger.LogInformation("Stripe successfully configured.");
}

// --------------------
// MIDDLEWARE
// --------------------
app.UseSerilogRequestLogging();

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseRequestLocalization(opts =>
{
    opts.AddSupportedCultures("en-US")
        .AddSupportedUICultures("en-US")
        .SetDefaultCulture("en-US");
});

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// --------------------
// ROUTES
// --------------------
app.MapControllerRoute(
    name: "catpage",
    pattern: "{category}/Page{productPage:int}",
    defaults: new { Controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "page",
    pattern: "Page{productPage:int}",
    defaults: new { Controller = "Home", action = "Index", productPage = 1 });

app.MapControllerRoute(
    name: "category",
    pattern: "{category}",
    defaults: new { Controller = "Home", action = "Index", productPage = 1 });

app.MapControllerRoute(
    name: "pagination",
    pattern: "Products/Page{productPage}",
    defaults: new { Controller = "Home", action = "Index", productPage = 1 });

app.MapDefaultControllerRoute();

app.MapRazorPages();

app.MapFallbackToPage("/admin/{*catchall}", "/Admin/Index");

// --------------------
// SEED DATA
// --------------------
SeedData.EnsurePopulated(app);
IdentitySeedData.EnsurePopulated(app);

// --------------------
// RUN
// --------------------
app.Run();