using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.PostsService.Core.Entities;

namespace Tailly.PostsService.Infrastructure.Configurations.Entities;

public class BannerConfiguration : IEntityTypeConfiguration<BannerEntity>
{
    public void Configure(EntityTypeBuilder<BannerEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(1000);

        builder.Property(x => x.LinkUrl)
               .HasMaxLength(1000);

        builder.Property(x => x.StartsAt)
               .IsRequired(false);

        builder.Property(x => x.EndsAt)
               .IsRequired(false);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();
    }
}