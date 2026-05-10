using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.Product;

namespace Tailly.ShopService.Infrastructure.DataAccess;

public static class ProductSeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var foodId = new Guid("11111111-1111-1111-1111-111111111111");
        var toysId = new Guid("22222222-2222-2222-2222-222222222222");
        var accessoriesId = new Guid("33333333-3333-3333-3333-333333333333");
        var groomingId = new Guid("44444444-4444-4444-4444-444444444444");
        var healthId = new Guid("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = foodId, Slug = "food", Title = "Корм и лакомства" },
            new CategoryEntity { Id = toysId, Slug = "toys", Title = "Игрушки и развлечения" },
            new CategoryEntity { Id = accessoriesId, Slug = "accessories", Title = "Аксессуары и одежда" },
            new CategoryEntity { Id = groomingId, Slug = "grooming", Title = "Груминг и уход" },
            new CategoryEntity { Id = healthId, Slug = "health", Title = "Здоровье и витамины" }
        );

        modelBuilder.Entity<ProductEntity>().HasData(
            new ProductEntity
            {
                Id = Guid.Parse("1b3647bc-92bd-483f-9aa4-d20130a619a9"),
                Slug = "royal-canin-adult-dog",
                Title = "Royal Canin Adult Dog Food",
                CategoryId = foodId,
                ShortDescription = "Сухой корм для взрослых собак",
                Description = "Премиальный полнорационный корм для собак старше 1 года. Поддерживает иммунитет и здоровье пищеварения.",
                Price = 2490M,
                OldPrice = 2790M,
                IsAvailable = true,
                StockQuantity = 87,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "Royal Canin",
                CountryOfOrigin = "Франция",
                ForWhom = "Собаки",
                Purpose = "Ежедневное питание",
                PetSize = "Все размеры",
                Material = "Сухой корм"
            },

            new ProductEntity
            {
                Id = Guid.Parse("81acec04-ac74-4f06-861a-261b64eaae85"),
                Slug = "whiskas-wet-chicken",
                Title = "Whiskas Wet Cat Food с курицей",
                CategoryId = foodId,
                ShortDescription = "Влажный корм для кошек",
                Description = "Нежные кусочки в соусе. Идеальный рацион для взрослых кошек.",
                Price = 89M,
                IsAvailable = true,
                StockQuantity = 320,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "Whiskas",
                CountryOfOrigin = "Россия",
                ForWhom = "Кошки",
                Purpose = "Ежедневное питание",
                PetSize = "Все размеры",
                Material = "Влажный корм"
            },

            new ProductEntity
            {
                Id = Guid.Parse("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"),
                Slug = "pedigree-puppy",
                Title = "Pedigree Puppy для щенков",
                CategoryId = foodId,
                ShortDescription = "Корм для щенков всех пород",
                Description = "Сбалансированное питание для правильного роста и развития щенков.",
                Price = 1890M,
                OldPrice = 2190M,
                IsAvailable = true,
                StockQuantity = 64,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "Pedigree",
                CountryOfOrigin = "Россия",
                ForWhom = "Щенки",
                Purpose = "Рост и развитие",
                PetSize = "Все размеры",
                Material = "Сухой корм"
            },

            new ProductEntity
            {
                Id = Guid.Parse("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"),
                Slug = "brit-care-salmon",
                Title = "Brit Care Grain-Free Salmon",
                CategoryId = foodId,
                ShortDescription = "Беззерновой корм с лососем",
                Description = "Гипоаллергенный корм для собак с чувствительным пищеварением.",
                Price = 3190M,
                IsAvailable = true,
                StockQuantity = 45,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "Brit Care",
                CountryOfOrigin = "Чехия",
                ForWhom = "Собаки",
                Purpose = "Гипоаллергенное питание",
                PetSize = "Все размеры",
                Material = "Сухой корм"
            },

            new ProductEntity
            {
                Id = Guid.Parse("f2539156-ab5d-4f0a-b542-dc9cc37c3239"),
                Slug = "bosch-dog-treats",
                Title = "Лакомство для собак Bosch",
                CategoryId = foodId,
                ShortDescription = "Натуральные лакомства",
                Description = "Полезные косточки и палочки для поощрения.",
                Price = 450M,
                OldPrice = 590M,
                IsAvailable = true,
                StockQuantity = 210,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "Bosch",
                CountryOfOrigin = "Германия",
                ForWhom = "Собаки",
                Purpose = "Поощрение и тренировка",
                PetSize = "Все размеры",
                Material = "Лакомства"
            },

            new ProductEntity
            {
                Id = Guid.Parse("3461d3bf-6549-4049-beb0-4651ab3a4f62"),
                Slug = "interactive-dog-ball",
                Title = "Интерактивный мяч для собак",
                CategoryId = toysId,
                ShortDescription = "Умная игрушка с движением",
                Description = "Автоматический мячик с несколькими режимами движения.",
                Price = 1290M,
                OldPrice = 1590M,
                IsAvailable = true,
                StockQuantity = 42,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "SmartPet",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Активные игры",
                PetSize = "Средний/Крупный",
                Material = "Пластик"
            },

            new ProductEntity
            {
                Id = Guid.Parse("c7b91735-1896-4dae-8b03-cfe3b90813f0"),
                Slug = "cat-feather-wand",
                Title = "Дразнилка с перьями для кошек",
                CategoryId = toysId,
                ShortDescription = "Любимая игрушка кошек",
                Description = "Лёгкая удочка с яркими перьями.",
                Price = 490M,
                IsAvailable = true,
                StockQuantity = 95,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "PlayCat",
                CountryOfOrigin = "Китай",
                ForWhom = "Кошки",
                Purpose = "Охота и активность",
                PetSize = "Все размеры",
                Material = "Перья + пластик"
            },

            new ProductEntity
            {
                Id = Guid.Parse("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"),
                Slug = "squeaky-plush-bear",
                Title = "Пищащий плюшевый медведь",
                CategoryId = toysId,
                ShortDescription = "Мягкая пищащая игрушка",
                Description = "Прочный плюш с пищалкой внутри.",
                Price = 890M,
                OldPrice = 1190M,
                IsAvailable = true,
                StockQuantity = 67,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "FunToys",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Перетягивание и жевание",
                PetSize = "Маленький/Средний",
                Material = "Плюш"
            },

            new ProductEntity
            {
                Id = Guid.Parse("9dac461d-d71a-4420-967f-dc397994f358"),
                Slug = "cat-laser-pointer",
                Title = "Лазерная указка для кошек",
                CategoryId = toysId,
                ShortDescription = "Лазер для активных игр",
                Description = "Классическая лазерная указка.",
                Price = 390M,
                IsAvailable = true,
                StockQuantity = 180,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "LaserPet",
                CountryOfOrigin = "Китай",
                ForWhom = "Кошки",
                Purpose = "Физическая и умственная нагрузка",
                PetSize = "Все размеры",
                Material = "Пластик"
            },

            new ProductEntity
            {
                Id = Guid.Parse("538fe373-3572-4dd1-b068-f923543f77d9"),
                Slug = "rope-tug-toy",
                Title = "Канатная игрушка для перетягивания",
                CategoryId = toysId,
                ShortDescription = "Крепкий канат",
                Description = "Прочный канат для игр с собакой.",
                Price = 690M,
                OldPrice = 890M,
                IsAvailable = true,
                StockQuantity = 53,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "StrongDog",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Перетягивание",
                PetSize = "Средний/Крупный",
                Material = "Хлопковый канат"
            },

            new ProductEntity
            {
                Id = Guid.Parse("a3746572-16ff-4cf3-af67-0b8673bffcf7"),
                Slug = "adjustable-dog-harness",
                Title = "Регулируемая шлейка для собак",
                CategoryId = accessoriesId,
                ShortDescription = "Комфортная шлейка",
                Description = "Мягкая и удобная шлейка для средних и крупных собак.",
                Price = 1690M,
                IsAvailable = true,
                StockQuantity = 28,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "ComfortWalk",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Выгул",
                PetSize = "Средний/Крупный",
                Material = "Нейлон + флис"
            },

            new ProductEntity
            {
                Id = Guid.Parse("37083c6d-2711-44de-90a0-b0bf2684dcba"),
                Slug = "xl-nylon-leash",
                Title = "Поводок нейлоновый XL",
                CategoryId = accessoriesId,
                ShortDescription = "Прочный поводок",
                Description = "Надёжный нейлоновый поводок для крупных собак.",
                Price = 1290M,
                OldPrice = 1490M,
                IsAvailable = false,
                StockQuantity = 33,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "ProLeash",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Выгул",
                PetSize = "Крупный",
                Material = "Нейлон"
            },

            new ProductEntity
            {
                Id = Guid.Parse("3080a42d-ab57-414e-8925-82775c1de349"),
                Slug = "warm-dog-jumpsuit",
                Title = "Теплый комбинезон для собак",
                CategoryId = accessoriesId,
                ShortDescription = "Зимняя одежда для собак",
                Description = "Утеплённый комбинезон с капюшоном.",
                Price = 2190M,
                OldPrice = 2590M,
                IsAvailable = true,
                StockQuantity = 19,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "WinterPet",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Защита от холода",
                PetSize = "Средний/Крупный",
                Material = "Полиэстер + синтепон"
            },

            new ProductEntity
            {
                Id = Guid.Parse("ea88eab3-06a9-4614-80f9-dd8d457a498d"),
                Slug = "ceramic-non-slip-bowl",
                Title = "Миска керамическая антискользящая",
                CategoryId = accessoriesId,
                ShortDescription = "Устойчивая миска",
                Description = "Керамическая миска с резиновым основанием.",
                Price = 790M,
                IsAvailable = true,
                StockQuantity = 124,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "CeramicPet",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки/Кошки",
                Purpose = "Кормление",
                PetSize = "Все размеры",
                Material = "Керамика + резина"
            },

            new ProductEntity
            {
                Id = Guid.Parse("23746c5a-0e90-4e6a-b902-445f92756b98"),
                Slug = "gps-pet-collar",
                Title = "Ошейник с GPS-трекером",
                CategoryId = accessoriesId,
                ShortDescription = "Умный ошейник",
                Description = "Ошейник с GPS и отслеживанием активности.",
                Price = 3490M,
                OldPrice = 3990M,
                IsAvailable = false,
                StockQuantity = 12,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "SmartTrack",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки",
                Purpose = "Отслеживание местоположения",
                PetSize = "Средний/Крупный",
                Material = "Нейлон"
            },

            new ProductEntity
            {
                Id = Guid.Parse("ba872d50-5c81-40d4-954e-0ecdb0916efb"),
                Slug = "dog-shampoo-aloe",
                Title = "Шампунь для собак с алоэ",
                CategoryId = groomingId,
                ShortDescription = "Гипоаллергенный шампунь",
                Description = "Натуральный шампунь с экстрактом алоэ.",
                Price = 790M,
                OldPrice = 990M,
                IsAvailable = false,
                StockQuantity = 47,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "PurePet",
                CountryOfOrigin = "Россия",
                ForWhom = "Собаки",
                Purpose = "Гигиена шерсти",
                PetSize = "Все размеры",
                Material = "Жидкий шампунь"
            },

            new ProductEntity
            {
                Id = Guid.Parse("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"),
                Slug = "pet-nail-clipper",
                Title = "Когтерез для животных",
                CategoryId = groomingId,
                ShortDescription = "Безопасный когтерез",
                Description = "Профессиональный когтерез с ограничителем.",
                Price = 690M,
                IsAvailable = true,
                StockQuantity = 55,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "GroomPro",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки/Кошки",
                Purpose = "Уход за когтями",
                PetSize = "Все размеры",
                Material = "Нержавеющая сталь"
            },

            new ProductEntity
            {
                Id = Guid.Parse("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"),
                Slug = "self-cleaning-cat-brush",
                Title = "Самоочищающаяся щетка для кошек",
                CategoryId = groomingId,
                ShortDescription = "Щётка с самоочисткой",
                Description = "Удобная щётка для удаления выпавшей шерсти.",
                Price = 590M,
                OldPrice = 790M,
                IsAvailable = true,
                StockQuantity = 68,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "FurCare",
                CountryOfOrigin = "Китай",
                ForWhom = "Кошки",
                Purpose = "Уход за шерстью",
                PetSize = "Все размеры",
                Material = "Пластик + металл"
            },

            new ProductEntity
            {
                Id = Guid.Parse("6d861eaf-a99e-4342-bbe2-c1529376bf06"),
                Slug = "dry-dog-shampoo",
                Title = "Сухой шампунь для собак",
                CategoryId = groomingId,
                ShortDescription = "Сухой шампунь без воды",
                Description = "Быстрое очищение шерсти без купания.",
                Price = 890M,
                IsAvailable = true,
                StockQuantity = 82,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "CleanPaw",
                CountryOfOrigin = "Россия",
                ForWhom = "Собаки",
                Purpose = "Сухое очищение",
                PetSize = "Все размеры",
                Material = "Сухой порошок"
            },

            new ProductEntity
            {
                Id = Guid.Parse("60a0315b-ebe9-46e4-b56e-acae29a356b6"),
                Slug = "pet-dental-kit",
                Title = "Набор для чистки зубов питомцам",
                CategoryId = groomingId,
                ShortDescription = "Гигиена полости рта",
                Description = "Полный набор для ухода за зубами.",
                Price = 1290M,
                OldPrice = 1490M,
                IsAvailable = true,
                StockQuantity = 37,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "DentalCare",
                CountryOfOrigin = "Китай",
                ForWhom = "Собаки/Кошки",
                Purpose = "Гигиена зубов",
                PetSize = "Все размеры",
                Material = "Пластик + металл"
            },

            new ProductEntity
            {
                Id = Guid.Parse("065f9a01-157f-4c29-9743-c9b1a562562d"),
                Slug = "joint-supplement-dogs",
                Title = "Витамины для суставов собак",
                CategoryId = healthId,
                ShortDescription = "Поддержка суставов",
                Description = "Комплекс для укрепления суставов и хрящей.",
                Price = 1490M,
                OldPrice = 1790M,
                IsAvailable = true,
                StockQuantity = 51,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "JointVet",
                CountryOfOrigin = "Германия",
                ForWhom = "Собаки",
                Purpose = "Здоровье суставов",
                PetSize = "Средний/Крупный",
                Material = "Таблетки"
            },
            new ProductEntity
            {
                Id = Guid.Parse("73db2a59-d927-4060-ab4a-a71a4ebd7f80"),
                Slug = "anti-parasite-drops-cats",
                Title = "Антипаразитарные капли для кошек",
                CategoryId = healthId,
                ShortDescription = "Защита от паразитов",
                Description = "Капли на холку от блох и клещей.",
                Price = 890M,
                IsAvailable = true,
                StockQuantity = 73,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "ParasiteGuard",
                CountryOfOrigin = "Россия",
                ForWhom = "Кошки",
                Purpose = "Защита от паразитов",
                PetSize = "Все размеры",
                Material = "Капли"
            },
            new ProductEntity
            {
                Id = Guid.Parse("6084749d-cc5a-4f5e-b83e-553b383c2091"),
                Slug = "probiotics-pet",
                Title = "Пробиотики для пищеварения",
                CategoryId = healthId,
                ShortDescription = "Нормализация микрофлоры",
                Description = "Пробиотический комплекс для ЖКТ.",
                Price = 1190M,
                OldPrice = 1390M,
                IsAvailable = false,
                StockQuantity = 29,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "BioGut",
                CountryOfOrigin = "Германия",
                ForWhom = "Собаки/Кошки",
                Purpose = "Нормализация пищеварения",
                PetSize = "Все размеры",
                Material = "Порошок"
            },
            new ProductEntity
            {
                Id = Guid.Parse("ee3b9279-da4b-421e-ac3e-b5b7ae287463"),
                Slug = "calcium-vitamin-d",
                Title = "Кальций с витамином D для щенков",
                CategoryId = healthId,
                ShortDescription = "Кальций для роста костей",
                Description = "Добавка для правильного формирования скелета.",
                Price = 690M,
                IsAvailable = true,
                StockQuantity = 95,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Brand = "BoneGrow",
                CountryOfOrigin = "Россия",
                ForWhom = "Щенки",
                Purpose = "Рост костей и зубов",
                PetSize = "Маленький/Средний",
                Material = "Таблетки"
            }
        );

        modelBuilder.Entity<ProductImageEntity>().HasData(
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("1b3647bc-92bd-483f-9aa4-d20130a619a9"), Url = "https://picsum.photos/id/237/800/800", Alt = "Royal Canin Adult Dog Food" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("81acec04-ac74-4f06-861a-261b64eaae85"), Url = "https://picsum.photos/id/1015/800/800", Alt = "Whiskas Wet Cat Food" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), Url = "https://picsum.photos/id/106/800/800", Alt = "Pedigree Puppy Food" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), Url = "https://picsum.photos/id/201/800/800", Alt = "Brit Care Grain-Free Salmon" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), Url = "https://picsum.photos/id/251/800/800", Alt = "Bosch Dog Treats" },

            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("3461d3bf-6549-4049-beb0-4651ab3a4f62"), Url = "https://picsum.photos/id/180/800/800", Alt = "Interactive Dog Ball" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("c7b91735-1896-4dae-8b03-cfe3b90813f0"), Url = "https://picsum.photos/id/133/800/800", Alt = "Cat Feather Wand Toy" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), Url = "https://picsum.photos/id/367/800/800", Alt = "Squeaky Plush Bear" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("9dac461d-d71a-4420-967f-dc397994f358"), Url = "https://picsum.photos/id/433/800/800", Alt = "Cat Laser Pointer" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("538fe373-3572-4dd1-b068-f923543f77d9"), Url = "https://picsum.photos/id/201/800/800", Alt = "Rope Tug Toy" },

            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("a3746572-16ff-4cf3-af67-0b8673bffcf7"), Url = "https://picsum.photos/id/180/800/800", Alt = "Adjustable Dog Harness" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("37083c6d-2711-44de-90a0-b0bf2684dcba"), Url = "https://picsum.photos/id/201/800/800", Alt = "XL Dog Leash" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("3080a42d-ab57-414e-8925-82775c1de349"), Url = "https://picsum.photos/id/251/800/800", Alt = "Warm Dog Jumpsuit" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("ea88eab3-06a9-4614-80f9-dd8d457a498d"), Url = "https://picsum.photos/id/367/800/800", Alt = "Ceramic Non-Slip Bowl" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("23746c5a-0e90-4e6a-b902-445f92756b98"), Url = "https://picsum.photos/id/433/800/800", Alt = "GPS Pet Collar" },

            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("ba872d50-5c81-40d4-954e-0ecdb0916efb"), Url = "https://picsum.photos/id/251/800/800", Alt = "Dog Shampoo" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), Url = "https://picsum.photos/id/367/800/800", Alt = "Pet Nail Clipper" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), Url = "https://picsum.photos/id/433/800/800", Alt = "Self-Cleaning Cat Brush" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("6d861eaf-a99e-4342-bbe2-c1529376bf06"), Url = "https://picsum.photos/id/180/800/800", Alt = "Dry Dog Shampoo" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("60a0315b-ebe9-46e4-b56e-acae29a356b6"), Url = "https://picsum.photos/id/201/800/800", Alt = "Pet Dental Kit" },

            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("065f9a01-157f-4c29-9743-c9b1a562562d"), Url = "https://picsum.photos/id/251/800/800", Alt = "Joint Supplement" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), Url = "https://picsum.photos/id/367/800/800", Alt = "Anti-Parasite Drops" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("6084749d-cc5a-4f5e-b83e-553b383c2091"), Url = "https://picsum.photos/id/433/800/800", Alt = "Probiotics" },
            new ProductImageEntity { Id = Guid.NewGuid(), ProductId = Guid.Parse("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), Url = "https://picsum.photos/id/180/800/800", Alt = "Calcium with Vitamin D" }
        );
    }
}