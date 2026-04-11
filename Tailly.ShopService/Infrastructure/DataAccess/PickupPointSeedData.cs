using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.Pickup;
using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Infrastructure.DataAccess;

public static class PickupPointSeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PickupPointEntity>().HasData(
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Тверской",
                Address = "Москва, ул. Тверская, 12, стр. 1",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Марьино",
                Address = "Москва, ул. Братиславская, 21, корп. 2",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Ленинском проспекте",
                Address = "Москва, Ленинский проспект, 45",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Химках",
                Address = "Москва, г. Химки, ул. Ленинградская, 29",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на ВДНХ",
                Address = "Москва, проспект Мира, 119, стр. 23",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Невском",
                Address = "Санкт-Петербург, Невский проспект, 56",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Купчино",
                Address = "Санкт-Петербург, ул. Будапештская, 18",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Московском проспекте",
                Address = "Санкт-Петербург, Московский проспект, 183",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК у станции метро Пионерская",
                Address = "Санкт-Петербург, пр. Испытателей, 15",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Малышева",
                Address = "Екатеринбург, ул. Малышева, 51",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Уралмаше",
                Address = "Екатеринбург, ул. Космонавтов, 11/1",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Красном проспекте",
                Address = "Новосибирск, Красный проспект, 52",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Академгородке",
                Address = "Новосибирск, ул. Ильича, 10",
                EstimatedDate = DateTime.UtcNow.AddDays(3)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК на Баумана",
                Address = "Казань, ул. Баумана, 27",
                EstimatedDate = DateTime.UtcNow.AddDays(1)
            },
            new PickupPointEntity
            {
                Id = Guid.NewGuid(),
                Provider = PickupProvider.Cdek,
                Title = "ПВЗ СДЭК в Советском районе",
                Address = "Казань, пр. Победы, 74",
                EstimatedDate = DateTime.UtcNow.AddDays(2)
            }
        );
    }
}