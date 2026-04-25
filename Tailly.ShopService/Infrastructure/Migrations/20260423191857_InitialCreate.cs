using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tailly.ShopService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PickupPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EstimatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProductSlug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecipientFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PickupPointId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_PickupPoints_PickupPointId",
                        column: x => x.PickupPointId,
                        principalTable: "PickupPoints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Alt = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReplyId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    House = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Apartment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAddresses_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ProductSlug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviewReplys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviewReplys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviewReplys_ProductReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ProductReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Slug", "Title" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "food", "Корм и лакомства" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "toys", "Игрушки и развлечения" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "accessories", "Аксессуары и одежда" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "grooming", "Груминг и уход" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "health", "Здоровье и витамины" }
                });

            migrationBuilder.InsertData(
                table: "PickupPoints",
                columns: new[] { "Id", "Address", "EstimatedDate", "Provider", "Title" },
                values: new object[,]
                {
                    { new Guid("001b20e9-da8e-4df3-9b13-1f9a66faa060"), "Казань, ул. Баумана, 27", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3418), 1, "ПВЗ СДЭК на Баумана" },
                    { new Guid("11cd81c7-f29c-407f-9339-27b935190bb7"), "Санкт-Петербург, ул. Будапештская, 18", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3405), 1, "ПВЗ СДЭК в Купчино" },
                    { new Guid("15d21bd4-f087-4967-baea-3a6ce21b924c"), "Москва, г. Химки, ул. Ленинградская, 29", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3400), 1, "ПВЗ СДЭК в Химках" },
                    { new Guid("255d1b87-5508-4edd-81f0-1df5d625ae64"), "Москва, Ленинский проспект, 45", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3399), 1, "ПВЗ СДЭК на Ленинском проспекте" },
                    { new Guid("3f339878-b053-40ea-95b8-1d8d5ab416f3"), "Екатеринбург, ул. Малышева, 51", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3410), 1, "ПВЗ СДЭК на Малышева" },
                    { new Guid("65f27ef8-f63c-4df5-ac81-568addddeae8"), "Екатеринбург, ул. Космонавтов, 11/1", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3413), 1, "ПВЗ СДЭК в Уралмаше" },
                    { new Guid("841f1b95-06fa-4a44-89cb-6ed22783a8da"), "Новосибирск, ул. Ильича, 10", new DateTime(2026, 4, 26, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3416), 1, "ПВЗ СДЭК в Академгородке" },
                    { new Guid("ac82ba08-21c1-40ab-a149-bb032c58c2c9"), "Санкт-Петербург, Невский проспект, 56", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3404), 1, "ПВЗ СДЭК на Невском" },
                    { new Guid("b1aa393b-e198-4128-bb9e-82aefc190549"), "Санкт-Петербург, пр. Испытателей, 15", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3408), 1, "ПВЗ СДЭК у станции метро Пионерская" },
                    { new Guid("ba898688-8898-4c81-acc8-a93d4728a19b"), "Москва, ул. Тверская, 12, стр. 1", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3385), 1, "ПВЗ СДЭК на Тверской" },
                    { new Guid("ca13495b-b993-4e84-a5ec-d4762b7eeeed"), "Москва, ул. Братиславская, 21, корп. 2", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3390), 1, "ПВЗ СДЭК в Марьино" },
                    { new Guid("d7030081-4195-4285-9bc9-c30fbaf4d29b"), "Казань, пр. Победы, 74", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3420), 1, "ПВЗ СДЭК в Советском районе" },
                    { new Guid("db7ef24f-be92-4094-90c7-59010320dac3"), "Санкт-Петербург, Московский проспект, 183", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3407), 1, "ПВЗ СДЭК на Московском проспекте" },
                    { new Guid("e9f7d0f4-e056-4f73-ba79-12db4da42305"), "Москва, проспект Мира, 119, стр. 23", new DateTime(2026, 4, 25, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3402), 1, "ПВЗ СДЭК на ВДНХ" },
                    { new Guid("f81b6aa7-0b95-4b67-a8cc-a2bde731648f"), "Новосибирск, Красный проспект, 52", new DateTime(2026, 4, 24, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3415), 1, "ПВЗ СДЭК на Красном проспекте" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsAvailable", "OldPrice", "Price", "Rating", "ReviewsCount", "ShortDescription", "Slug", "StockQuantity", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3171), "Комплекс для укрепления суставов и хрящей.", true, 1790m, 1490m, 0m, 0, "Поддержка суставов", "joint-supplement-dogs", 51, "Витамины для суставов собак", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3171) },
                    { new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3018), "Премиальный полнорационный корм для собак старше 1 года. Поддерживает иммунитет и здоровье пищеварения.", true, 2790m, 2490m, 0m, 0, "Сухой корм для взрослых собак", "royal-canin-adult-dog", 87, "Royal Canin Adult Dog Food", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3019) },
                    { new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3143), "Ошейник с GPS и отслеживанием активности.", false, 3990m, 3490m, 0m, 0, "Умный ошейник", "gps-pet-collar", 12, "Ошейник с GPS-трекером", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3144) },
                    { new Guid("3080a42d-ab57-414e-8925-82775c1de349"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3135), "Утеплённый комбинезон с капюшоном.", true, 2590m, 2190m, 0m, 0, "Зимняя одежда для собак", "warm-dog-jumpsuit", 19, "Теплый комбинезон для собак", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3136) },
                    { new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3108), "Автоматический мячик с несколькими режимами движения.", true, 1590m, 1290m, 0m, 0, "Умная игрушка с движением", "interactive-dog-ball", 42, "Интерактивный мяч для собак", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3108) },
                    { new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3131), "Надёжный нейлоновый поводок для крупных собак.", false, 1490m, 1290m, 0m, 0, "Прочный поводок", "xl-nylon-leash", 33, "Поводок нейлоновый XL", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3131) },
                    { new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3155), "Удобная щётка для удаления выпавшей шерсти.", true, 790m, 590m, 0m, 0, "Щётка с самоочисткой", "self-cleaning-cat-brush", 68, "Самоочищающаяся щетка для кошек", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3155) },
                    { new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3123), "Прочный канат для игр с собакой.", true, 890m, 690m, 0m, 0, "Крепкий канат", "rope-tug-toy", 53, "Канатная игрушка для перетягивания", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3123) },
                    { new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3179), "Пробиотический комплекс для ЖКТ.", false, 1390m, 1190m, 0m, 0, "Нормализация микрофлоры", "probiotics-pet", 29, "Пробиотики для пищеварения", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3179) },
                    { new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3167), "Полный набор для ухода за зубами.", true, 1490m, 1290m, 0m, 0, "Гигиена полости рта", "pet-dental-kit", 37, "Набор для чистки зубов питомцам", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3167) },
                    { new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3151), "Профессиональный когтерез с ограничителем.", true, null, 690m, 0m, 0, "Безопасный когтерез", "pet-nail-clipper", 55, "Когтерез для животных", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3151) },
                    { new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3163), "Быстрое очищение шерсти без купания.", true, null, 890m, 0m, 0, "Сухой шампунь без воды", "dry-dog-shampoo", 82, "Сухой шампунь для собак", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3163) },
                    { new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3175), "Капли на холку от блох и клещей.", true, null, 890m, 0m, 0, "Защита от паразитов", "anti-parasite-drops-cats", 73, "Антипаразитарные капли для кошек", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3175) },
                    { new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3023), "Нежные кусочки в соусе. Идеальный рацион для взрослых кошек.", true, null, 89m, 0m, 0, "Влажный корм для кошек", "whiskas-wet-chicken", 320, "Whiskas Wet Cat Food с курицей", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3023) },
                    { new Guid("9dac461d-d71a-4420-967f-dc397994f358"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3119), "Классическая лазерная указка.", true, null, 390m, 0m, 0, "Лазер для активных игр", "cat-laser-pointer", 180, "Лазерная указка для кошек", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3119) },
                    { new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3127), "Мягкая и удобная шлейка для средних и крупных собак.", true, null, 1690m, 0m, 0, "Комфортная шлейка", "adjustable-dog-harness", 28, "Регулируемая шлейка для собак", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3127) },
                    { new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3028), "Сбалансированное питание для правильного роста и развития щенков.", true, 2190m, 1890m, 0m, 0, "Корм для щенков всех пород", "pedigree-puppy", 64, "Pedigree Puppy для щенков", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3029) },
                    { new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3147), "Натуральный шампунь с экстрактом алоэ.", false, 990m, 790m, 0m, 0, "Гипоаллергенный шампунь", "dog-shampoo-aloe", 47, "Шампунь для собак с алоэ", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3148) },
                    { new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3111), "Лёгкая удочка с яркими перьями.", true, null, 490m, 0m, 0, "Любимая игрушка кошек", "cat-feather-wand", 95, "Дразнилка с перьями для кошек", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3112) },
                    { new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3115), "Прочный плюш с пищалкой внутри.", true, 1190m, 890m, 0m, 0, "Мягкая пищащая игрушка", "squeaky-plush-bear", 67, "Пищащий плюшевый медведь", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3116) },
                    { new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3100), "Гипоаллергенный корм для собак с чувствительным пищеварением.", true, null, 3190m, 0m, 0, "Беззерновой корм с лососем", "brit-care-salmon", 45, "Brit Care Grain-Free Salmon", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3100) },
                    { new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3139), "Керамическая миска с резиновым основанием.", true, null, 790m, 0m, 0, "Устойчивая миска", "ceramic-non-slip-bowl", 124, "Миска керамическая антискользящая", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3139) },
                    { new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3183), "Добавка для правильного формирования скелета.", true, null, 690m, 0m, 0, "Кальций для роста костей", "calcium-vitamin-d", 95, "Кальций с витамином D для щенков", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3183) },
                    { new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3104), "Полезные косточки и палочки для поощрения.", true, 590m, 450m, 0m, 0, "Натуральные лакомства", "bosch-dog-treats", 210, "Лакомство для собак Bosch", new DateTime(2026, 4, 23, 19, 18, 56, 772, DateTimeKind.Utc).AddTicks(3104) }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "Alt", "ProductId", "Url" },
                values: new object[,]
                {
                    { new Guid("031e4348-ad20-45c9-bf5e-69739db6c067"), "Ceramic Non-Slip Bowl", new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("20fa51f0-c6b3-4b00-8192-71dba1532d11"), "Adjustable Dog Harness", new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("37c2b67d-e2e2-4627-8acd-e3a27b4b520e"), "Probiotics", new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("3aa7f8c8-feee-47c7-80ef-3be3392d37c2"), "Interactive Dog Ball", new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("3bc21336-640e-47fd-9acf-cbf0fb9d6a67"), "GPS Pet Collar", new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("4258a8e8-1ec8-40cc-8091-4416060fca18"), "Pet Dental Kit", new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("425c8922-ee0c-4616-beb4-320265e69d20"), "Joint Supplement", new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("4fbf99d2-fdf0-4ba0-8718-8a453b54e1d5"), "Rope Tug Toy", new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("59c25f23-d863-42c1-9edf-9f1a655ee665"), "Dry Dog Shampoo", new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("5e4e514e-cf87-4d19-a35f-01bb7eb4b722"), "Dog Shampoo", new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("60e32ff9-1b6d-4e36-a62d-04cacab8ca00"), "Self-Cleaning Cat Brush", new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("64e6056f-7ef7-4669-8bc2-4d13e452b224"), "Royal Canin Adult Dog Food", new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), "https://picsum.photos/id/237/800/800" },
                    { new Guid("69f03536-596f-432f-8b9d-71fd0d463b55"), "Squeaky Plush Bear", new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("6a5742ce-f129-45cb-af68-694243f095b2"), "Warm Dog Jumpsuit", new Guid("3080a42d-ab57-414e-8925-82775c1de349"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("6b871933-fb06-4429-97ba-5f5b20a34797"), "Pet Nail Clipper", new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("6d87e571-57c5-4202-90e9-125101b1320b"), "XL Dog Leash", new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("7012ac2b-504e-4607-a59b-300f828be474"), "Calcium with Vitamin D", new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("7ae01aaa-7f8b-4f9d-92d5-a557b499c17a"), "Brit Care Grain-Free Salmon", new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("8bb4644f-e5a2-4af0-8fcf-d4c100bca07a"), "Cat Laser Pointer", new Guid("9dac461d-d71a-4420-967f-dc397994f358"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("a4d3ee01-4626-4c38-a9be-3b567e2e58c2"), "Bosch Dog Treats", new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("c4fef779-aa19-42d2-a577-d5a9b7bfce91"), "Whiskas Wet Cat Food", new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), "https://picsum.photos/id/1015/800/800" },
                    { new Guid("c69f1025-b12c-4d79-98ca-a3d6480e5527"), "Cat Feather Wand Toy", new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), "https://picsum.photos/id/133/800/800" },
                    { new Guid("ebcf4838-22c4-4a48-92b9-b1222f0af04d"), "Anti-Parasite Drops", new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("ef596dac-71f7-4dfe-a688-54cad5f83050"), "Pedigree Puppy Food", new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), "https://picsum.photos/id/106/800/800" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_SessionId",
                table: "Carts",
                column: "SessionId",
                unique: true,
                filter: "\"SessionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true,
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_SessionId_ProductId",
                table: "Favorites",
                columns: new[] { "SessionId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_ProductId",
                table: "Favorites",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddresses_OrderId",
                table: "OrderAddresses",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerUserId",
                table: "Orders",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PickupPointId",
                table: "Orders",
                column: "PickupPointId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviewReplys_ReviewId",
                table: "ProductReviewReplys",
                column: "ReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_OrderId",
                table: "ProductReviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId_OrderId_ProductId",
                table: "ProductReviews",
                columns: new[] { "UserId", "OrderId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsAvailable",
                table: "Products",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Rating",
                table: "Products",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "OrderAddresses");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductReviewReplys");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "PickupPoints");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
