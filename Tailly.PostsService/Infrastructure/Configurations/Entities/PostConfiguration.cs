using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.PostsService.Core.Entities;

namespace Tailly.PostsService.Infrastructure.Configurations.Entities;

public class PostConfiguration : IEntityTypeConfiguration<PostEntity>
{
    public void Configure(EntityTypeBuilder<PostEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Content)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.Property(x => x.PublishedAt)
               .IsRequired(false);

        builder.Property(x => x.CreatedBy)
               .IsRequired();

        builder.HasMany(x => x.Images)
               .WithOne(x => x.Post)
               .HasForeignKey(x => x.PostId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
               .WithOne(x => x.Post)
               .HasForeignKey(x => x.PostId);
    }
}