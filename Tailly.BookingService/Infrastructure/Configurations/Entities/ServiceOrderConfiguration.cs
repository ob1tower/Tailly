using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.BookingService.Core.Entities;

namespace Tailly.BookingService.Infrastructure.Configurations.Entities;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrderEntity>
{
    public void Configure(EntityTypeBuilder<ServiceOrderEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.ClientName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.SpecialistName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.SpecialistSlug)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.PetName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.ServiceTitle)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(x => x.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.PriceUnit) 
               .IsRequired();

        builder.Property(x => x.Comment)
               .HasMaxLength(1000);

        builder.Property(x => x.CancelReason)
               .HasMaxLength(500);

        builder.HasIndex(x => x.Number)
               .IsUnique();

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.SpecialistId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.ServiceSnapshot)
               .WithOne(x => x.Order)
               .HasForeignKey<ServiceOrderServiceSnapshotEntity>(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Review)
               .WithOne(x => x.Order)
               .HasForeignKey<ServiceOrderReviewEntity>(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}