using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Services;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Services;

public class BookedSlotServiceConfiguration : IEntityTypeConfiguration<BookedSlotServiceEntity>
{
    public void Configure(EntityTypeBuilder<BookedSlotServiceEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.BookedSlot)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.BookedSlotId);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.BookedSlots)
            .HasForeignKey(x => x.ServiceId);
    }
}