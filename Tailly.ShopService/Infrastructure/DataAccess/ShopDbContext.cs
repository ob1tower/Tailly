using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.Cart;
using Tailly.ShopService.Core.Entities.Order;
using Tailly.ShopService.Core.Entities.Pickup;
using Tailly.ShopService.Core.Entities.Product;
using Tailly.ShopService.Core.Entities.User;
using Tailly.ShopService.Infrastructure.Configurations.Entities;

namespace Tailly.ShopService.Infrastructure.DataAccess;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderAddressConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        modelBuilder.ApplyConfiguration(new PickupPointConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewReplyConfiguration());
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new FavoriteConfiguration());

        ProductSeedData.Seed(modelBuilder);
        PickupPointSeedData.Seed(modelBuilder);
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    public DbSet<OrderAddressEntity> OrderAddresses { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductImageEntity> ProductImages { get; set; }
    public DbSet<PickupPointEntity> PickupPoints { get; set; }
    public DbSet<ProductReviewEntity> ProductReviews { get; set; }
    public DbSet<ProductReviewReplyEntity> ProductReviewReplys { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<CartItemEntity> CartItems { get; set; }
    public DbSet<FavoriteEntity> Favorites { get; set; }
}