using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.Pickup;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class PickupPointConfiguration : IEntityTypeConfiguration<PickupPointEntity>
{
    public void Configure(EntityTypeBuilder<PickupPointEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
               .IsRequired();

        builder.Property(x => x.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Address)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.EstimatedDate)
               .IsRequired();
    }
}