using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.ClientProfileService.Core.Entities;

namespace Tailly.ClientProfileService.Infrastructure.Configurations.Entities;

public class PetEntityConfiguration : IEntityTypeConfiguration<PetEntity>
{
    public void Configure(EntityTypeBuilder<PetEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).IsRequired();

        builder.Property(x => x.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.PhotoUrl)
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.Type)
               .IsRequired(false);

        builder.Property(x => x.AgeYears).IsRequired();
        builder.Property(x => x.AgeMonths).IsRequired();

        builder.Property(x => x.Size);
        builder.Property(x => x.Gender);
        builder.Property(x => x.ToOtherPets);
        builder.Property(x => x.ToKidsUnder10);
        builder.Property(x => x.StaysHomeAlone);
        builder.Property(x => x.Vaccinated);

        builder.Property(x => x.Notes)
               .HasMaxLength(1000)
               .IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.ClientProfile)
               .WithMany(x => x.Pets)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.BreedId)
               .IsRequired(false);

        builder.HasOne(x => x.Breed)
               .WithMany()
               .HasForeignKey(x => x.BreedId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.BreedId);
    }
}