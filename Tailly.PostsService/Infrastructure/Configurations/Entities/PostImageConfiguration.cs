using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.PostsService.Core.Entities;

namespace Tailly.PostsService.Infrastructure.Configurations.Entities;

public class PostImageConfiguration : IEntityTypeConfiguration<PostImageEntity>
{
    public void Configure(EntityTypeBuilder<PostImageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(x => x.IsCover)
               .IsRequired();

        builder.HasOne(x => x.Post)
               .WithMany(x => x.Images)
               .HasForeignKey(x => x.PostId);
    }
}