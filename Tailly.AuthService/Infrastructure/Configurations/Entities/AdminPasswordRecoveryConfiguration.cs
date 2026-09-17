using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Core.Entities;

namespace Tailly.AuthService.Infrastructure.Configurations.Entities;

public class AdminPasswordRecoveryConfiguration : IEntityTypeConfiguration<AdminPasswordRecoveryEntity>
{
    public void Configure(EntityTypeBuilder<AdminPasswordRecoveryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(x => x.RequestedAt)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.ProcessedAt)
               .IsRequired(false);

        builder.Property(x => x.TemporaryPassword)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.ProcessedBy)
               .IsRequired(false);

        builder.HasIndex(x => x.Email);
    }
}