using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.PostsService.Core.Entities;

namespace Tailly.PostsService.Infrastructure.Configurations.Entities;

public class PostTagConfiguration : IEntityTypeConfiguration<PostTagEntity>
{
    public void Configure(EntityTypeBuilder<PostTagEntity> builder)
    {
        builder.HasKey(x => new { x.PostId, x.TagId });

        builder.HasOne(x => x.Post)
               .WithMany(x => x.Tags)
               .HasForeignKey(x => x.PostId);

        builder.HasOne(x => x.Tag)
               .WithMany(x => x.PostTags)
               .HasForeignKey(x => x.TagId);
    }
}