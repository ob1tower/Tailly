using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.AuthService.Entities;
using Tailly.AuthService.Enums;

namespace Tailly.AuthService.Configurations.Entities;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasData(
            Enum.GetValues<RoleType>()
                .Select(r => new RoleEntity
                {
                    Id = (int)r,
                    Name = r.ToString()
                })
        );
    }
}
