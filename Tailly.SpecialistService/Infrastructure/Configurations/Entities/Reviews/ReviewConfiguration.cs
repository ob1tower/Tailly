using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Reviews;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Reviews;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Text)
            .HasMaxLength(3000);

        builder.Property(x => x.ServiceTitle)
            .HasMaxLength(200);

        builder.Property(x => x.PetName)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}