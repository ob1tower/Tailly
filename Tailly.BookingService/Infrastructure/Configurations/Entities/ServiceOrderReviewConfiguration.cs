using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.BookingService.Core.Entities;

namespace Tailly.BookingService.Infrastructure.Configurations.Entities;

public class ServiceOrderReviewConfiguration : IEntityTypeConfiguration<ServiceOrderReviewEntity>
{
    public void Configure(EntityTypeBuilder<ServiceOrderReviewEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
               .HasMaxLength(2000)
               .IsRequired();

        builder.Property(x => x.Photos)
       .HasConversion(
           v => string.Join(";", v),
           v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
       .HasDefaultValueSql("'{}'");

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasIndex(x => x.OrderId);
    }
}