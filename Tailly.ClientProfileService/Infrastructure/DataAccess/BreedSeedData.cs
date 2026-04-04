using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Core.Entities;
using Tailly.ClientProfileService.Core.Enums;

namespace Tailly.ClientProfileService.Infrastructure.DataAccess;

public static class BreedSeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BreedEntity>().HasData(
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Dog, Title = "Лабрадор" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Dog, Title = "Немецкая овчарка" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Dog, Title = "Французский бульдог" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Dog, Title = "Корги" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Dog, Title = "Хаски" },

            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Cat, Title = "Мейн-кун" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Cat, Title = "Шотландская вислоухая" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Cat, Title = "Сиамская" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Cat, Title = "Британская короткошерстная" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Cat, Title = "Рэгдолл" },

            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Bird, Title = "Волнистый попугай" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Bird, Title = "Корелла" },

            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Rabbit, Title = "Лев" },
            new BreedEntity { Id = Guid.NewGuid(), Type = PetType.Rabbit, Title = "Баран" }
        );
    }
}