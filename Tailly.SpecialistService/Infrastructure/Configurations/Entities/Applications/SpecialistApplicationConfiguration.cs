using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tailly.SpecialistService.Core.Entities.Applications;

namespace Tailly.SpecialistService.Infrastructure.Configurations.Entities.Applications;

public class SpecialistApplicationConfiguration : IEntityTypeConfiguration<SpecialistApplicationEntity>
{
    public void Configure(EntityTypeBuilder<SpecialistApplicationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.MiddleName)
               .HasMaxLength(100);

        builder.Property(x => x.Phone)
               .HasMaxLength(20);

        builder.Property(x => x.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.About)
               .HasMaxLength(2000);

        builder.Property(x => x.ExperienceYears)
               .HasMaxLength(100);

        builder.Property(x => x.AnimalTypes)
               .HasMaxLength(500);

        builder.Property(x => x.ServiceFormats)
               .HasMaxLength(500);

        builder.Property(x => x.HousingType)
               .HasMaxLength(50);

        builder.Property(x => x.DistrictPreferences)
               .HasMaxLength(500);

        builder.Property(x => x.SchedulePreferences)
               .HasMaxLength(500);

        builder.Property(x => x.PortfolioUrl)
               .HasMaxLength(500);

        builder.Property(x => x.Motivation)
               .HasMaxLength(2000);

        builder.Property(x => x.AdditionalInfo)
               .HasMaxLength(2000);

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.Property(x => x.ReviewComment)
               .IsRequired(false);

        builder.Property(x => x.ReviewedBy)
               .IsRequired(false);

        builder.Property(x => x.InterviewNote)
               .HasMaxLength(1000);

        builder.Property(x => x.RejectionReason)
               .HasMaxLength(1000);

        builder.Property(x => x.CreatedSpecialistSlug)
               .HasMaxLength(150);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
    }
}