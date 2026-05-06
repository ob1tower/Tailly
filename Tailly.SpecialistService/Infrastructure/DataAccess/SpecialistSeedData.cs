using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Services;
using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;

namespace Tailly.SpecialistService.Infrastructure.DataAccess;

public static class SpecialistSeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var annaId = new Guid("11111111-1111-1111-1111-111111111111");
        var dimaId = new Guid("22222222-2222-2222-2222-222222222222");
        var mariaId = new Guid("33333333-3333-3333-3333-333333333333");

        var annaDetailsId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var dimaDetailsId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var mariaDetailsId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");

        modelBuilder.Entity<SpecialistEntity>().HasData(
            new SpecialistEntity
            {
                Id = annaId,
                UserId = new Guid("438a4a9e-c17b-439d-ab8b-007ed01d2ac8"),
                Slug = "anna-petcare",
                FirstName = "Анна",
                LastName = "Смирнова",
                City = "Москва",
                District = "Сокольники",
                Phone = "+79161234567",
                Email = "anna@example.com",
                AvatarUrl = "/uploads/avatars/anna.jpg",
                ExperienceYears = 5,
                Rating = 4.8m,
                ReviewsCount = 42,
                CompletedOrdersCount = 87,
                RepeatOrdersCount = 24,
                Latitude = 55.7931,
                Longitude = 37.6778,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new SpecialistEntity
            {
                Id = dimaId,
                UserId = Guid.NewGuid(),
                Slug = "dima-dogwalker",
                FirstName = "Дмитрий",
                LastName = "Кузнецов",
                City = "Москва",
                District = "Марьино",
                Phone = "+79162345678",
                Email = "dima@example.com",
                ExperienceYears = 3,
                Rating = 4.6m,
                ReviewsCount = 28,
                CompletedOrdersCount = 45,
                RepeatOrdersCount = 12,
                Latitude = 55.6494,
                Longitude = 37.7430,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new SpecialistEntity
            {
                Id = mariaId,
                UserId = Guid.NewGuid(),
                Slug = "maria-grooming",
                FirstName = "Мария",
                LastName = "Попова",
                City = "Санкт-Петербург",
                District = "Центральный",
                Phone = "+79213456789",
                Email = "maria@example.com",
                ExperienceYears = 7,
                Rating = 4.9m,
                ReviewsCount = 67,
                CompletedOrdersCount = 112,
                RepeatOrdersCount = 35,
                Latitude = 59.9386,
                Longitude = 30.3141,
                CreatedAt = DateTime.UtcNow.AddMonths(-8)
            }
        );

        modelBuilder.Entity<DetailsEntity>().HasData(
            new DetailsEntity
            {
                Id = annaDetailsId,
                SpecialistId = annaId,
                HousingType = HousingType.Apartment,
                HasChildrenUnderTen = ChildrenPolicy.No,
                About = "Люблю животных, имею двух своих собак. Занимаюсь выгулом и передержкой уже более 5 лет."
            },
            new DetailsEntity
            {
                Id = dimaDetailsId,
                SpecialistId = dimaId,
                HousingType = HousingType.House,
                HasChildrenUnderTen = ChildrenPolicy.Yes,
                About = "Профессиональный выгульщик с опытом работы с крупными породами."
            },
            new DetailsEntity
            {
                Id = mariaDetailsId,
                SpecialistId = mariaId,
                HousingType = HousingType.Apartment,
                HasChildrenUnderTen = ChildrenPolicy.Sometimes,
                About = "Профессиональный грумер собак и кошек с 7-летним стажем."
            }
        );

        modelBuilder.Entity<PetSizeEntity>().HasData(
            new PetSizeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetSize = PetSize.Kg2To5
            },
            new PetSizeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetSize = PetSize.Kg5To10
            },
            new PetSizeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = dimaDetailsId,
                PetSize = PetSize.Kg10To20
            },
            new PetSizeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = dimaDetailsId,
                PetSize = PetSize.Over20Kg
            }
        );

        modelBuilder.Entity<PetAgeEntity>().HasData(
            new PetAgeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetAge = PetAge.Adult
            },
            new PetAgeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetAge = PetAge.Senior
            }
        );

        modelBuilder.Entity<PetTypeEntity>().HasData(
            new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetType = PetType.Dog
            },
            new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = annaDetailsId,
                PetType = PetType.Cat
            },
            new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = dimaDetailsId,
                PetType = PetType.Dog
            },
            new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = mariaDetailsId,
                PetType = PetType.Dog
            },
            new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = mariaDetailsId,
                PetType = PetType.Cat
            }
        );

        modelBuilder.Entity<ServiceEntity>().HasData(
            new ServiceEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = annaId,
                Name = ServiceType.Walking,
                Description = "Выгул собаки 60 минут",
                Price = 1200,
                PriceUnit = ServicePriceUnit.Walk
            },
            new ServiceEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = annaId,
                Name = ServiceType.Boarding,
                Description = "Комфортная передержка в квартире",
                Price = 2500,
                PriceUnit = ServicePriceUnit.Day
            },
            new ServiceEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = dimaId,
                Name = ServiceType.Photoshoot,
                Description = "Выгул с элементами дрессировки",
                Price = 1800,
                PriceUnit = ServicePriceUnit.Hour
            },
            new ServiceEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = mariaId,
                Name = ServiceType.Grooming,
                Description = "Полный груминг",
                Price = 3500,
                PriceUnit = ServicePriceUnit.Service
            }
        );

        modelBuilder.Entity<SpecialistGalleryEntity>().HasData(
            new SpecialistGalleryEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = annaId,
                Order = 1,
                ImageUrl = "/uploads/gallery/anna1.jpg",
                Alt = "Анна с питомцем"
            },
            new SpecialistGalleryEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = annaId,
                Order = 2,
                ImageUrl = "/uploads/gallery/anna2.jpg",
                Alt = "Анна на прогулке"
            }
        );

        modelBuilder.Entity<CalendarEntity>().HasData(
            new CalendarEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = annaId,
                Timezone = "Europe/Moscow"
            },
            new CalendarEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = dimaId,
                Timezone = "Europe/Moscow"
            },
            new CalendarEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = mariaId,
                Timezone = "Europe/Moscow"
            }
        );
    }
}