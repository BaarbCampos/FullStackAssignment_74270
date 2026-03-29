using Microsoft.EntityFrameworkCore;
using SportsStore.OrderApi.Models;

namespace SportsStore.OrderApi.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.CustomerEmail)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Football", Price = 29.99m, StockQuantity = 50 },
            new Product { Id = 2, Name = "Basketball Shoes", Price = 89.99m, StockQuantity = 20 },
            new Product { Id = 3, Name = "Tennis Racket", Price = 120.00m, StockQuantity = 15 },
            new Product { Id = 4, Name = "Gym Bottle", Price = 12.50m, StockQuantity = 100 }
        );
    }
}