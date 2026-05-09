using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        var photosComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );

        builder.Property(x => x.Photos)
               .HasConversion(
                   v => string.Join(";", v),
                   v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
               .HasDefaultValueSql("'{}'")
               .Metadata.SetValueComparer(photosComparer);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasIndex(x => x.OrderId);
    }
}