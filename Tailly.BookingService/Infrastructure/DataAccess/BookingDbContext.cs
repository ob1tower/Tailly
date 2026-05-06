using Microsoft.EntityFrameworkCore;
using Tailly.BookingService.Core.Entities;
using Tailly.BookingService.Infrastructure.Configurations.Entities;

namespace Tailly.BookingService.Infrastructure.DataAccess;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
       : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ServiceOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOrderReviewConfiguration());
    }

    public DbSet<ServiceOrderEntity> ServiceOrders { get; set; }
    public DbSet<ServiceOrderReviewEntity> ServiceOrderReviews { get; set; }
}