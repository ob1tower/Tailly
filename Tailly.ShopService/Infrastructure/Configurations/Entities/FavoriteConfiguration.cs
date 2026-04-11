using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ShopService.Core.Entities.User;

namespace Tailly.ShopService.Infrastructure.Configurations.Entities;

public class FavoriteConfiguration : IEntityTypeConfiguration<FavoriteEntity>
{
    public void Configure(EntityTypeBuilder<FavoriteEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddedAt)
               .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.ProductId })
               .IsUnique();

        builder.HasIndex(x => new { x.SessionId, x.ProductId })
               .IsUnique();
    }
}