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
                    CanBeCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    RecipientFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    { new Guid("027286f6-ee4d-4c0c-9e7a-cbd0af7ea442"), "Екатеринбург, ул. Малышева, 51", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3488), 1, "ПВЗ СДЭК на Малышева" },
                    { new Guid("1160b6c1-94f8-4bce-acd0-983984127815"), "Москва, ул. Братиславская, 21, корп. 2", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3472), 1, "ПВЗ СДЭК в Марьино" },
                    { new Guid("1205a018-3ac4-458f-b1ad-96747a0cba5c"), "Новосибирск, ул. Ильича, 10", new DateTime(2026, 4, 14, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3493), 1, "ПВЗ СДЭК в Академгородке" },
                    { new Guid("18133511-4538-415e-b759-4f06cf5c1d39"), "Москва, г. Химки, ул. Ленинградская, 29", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3475), 1, "ПВЗ СДЭК в Химках" },
                    { new Guid("258dcde6-c023-4b42-b10a-fd405ae55671"), "Екатеринбург, ул. Космонавтов, 11/1", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3489), 1, "ПВЗ СДЭК в Уралмаше" },
                    { new Guid("8260749b-f7c1-4949-8274-7b2cd9585024"), "Санкт-Петербург, Московский проспект, 183", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3485), 1, "ПВЗ СДЭК на Московском проспекте" },
                    { new Guid("86390af9-c7bd-428b-a506-a3d8f2584688"), "Санкт-Петербург, ул. Будапештская, 18", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3483), 1, "ПВЗ СДЭК в Купчино" },
                    { new Guid("a2dd8080-ecac-4f57-adf5-2f6c67dd1f3a"), "Москва, проспект Мира, 119, стр. 23", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3477), 1, "ПВЗ СДЭК на ВДНХ" },
                    { new Guid("ac0d7301-e88e-4db4-88e4-df62f317fd53"), "Москва, Ленинский проспект, 45", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3474), 1, "ПВЗ СДЭК на Ленинском проспекте" },
                    { new Guid("c85f7634-b297-412d-ba59-e55eeab519c0"), "Казань, ул. Баумана, 27", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3496), 1, "ПВЗ СДЭК на Баумана" },
                    { new Guid("d0f1984e-d1cb-4501-9029-c35bba68aeb7"), "Санкт-Петербург, пр. Испытателей, 15", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3486), 1, "ПВЗ СДЭК у станции метро Пионерская" },
                    { new Guid("d5802f5a-ad2d-4a07-8973-febfb61c323d"), "Москва, ул. Тверская, 12, стр. 1", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3470), 1, "ПВЗ СДЭК на Тверской" },
                    { new Guid("f4665b4f-ef7d-46a4-8fd5-dbfe7a137c32"), "Новосибирск, Красный проспект, 52", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3491), 1, "ПВЗ СДЭК на Красном проспекте" },
                    { new Guid("f63b286f-dc06-4f74-b7ef-01c3e3598b03"), "Санкт-Петербург, Невский проспект, 56", new DateTime(2026, 4, 12, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3481), 1, "ПВЗ СДЭК на Невском" },
                    { new Guid("fd250fe9-51d1-4971-8769-cb38d81165ea"), "Казань, пр. Победы, 74", new DateTime(2026, 4, 13, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3498), 1, "ПВЗ СДЭК в Советском районе" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsAvailable", "OldPrice", "Price", "Rating", "ReviewsCount", "ShortDescription", "Slug", "StockQuantity", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3256), "Комплекс для укрепления суставов и хрящей.", true, 1790m, 1490m, 4.7m, 142, "Поддержка суставов", "joint-supplement-dogs", 51, "Витамины для суставов собак", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3257) },
                    { new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3127), "Премиальный полнорационный корм для собак старше 1 года. Поддерживает иммунитет и здоровье пищеварения.", true, 2790m, 2490m, 4.9m, 234, "Сухой корм для взрослых собак", "royal-canin-adult-dog", 87, "Royal Canin Adult Dog Food", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3128) },
                    { new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3188), "Ошейник с GPS и отслеживанием активности.", true, 3990m, 3490m, 4.4m, 45, "Умный ошейник", "gps-pet-collar", 12, "Ошейник с GPS-трекером", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3188) },
                    { new Guid("3080a42d-ab57-414e-8925-82775c1de349"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3179), "Утеплённый комбинезон с капюшоном.", true, 2590m, 2190m, 4.9m, 76, "Зимняя одежда для собак", "warm-dog-jumpsuit", 19, "Теплый комбинезон для собак", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3180) },
                    { new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3149), "Автоматический мячик с несколькими режимами движения.", true, 1590m, 1290m, 4.6m, 156, "Умная игрушка с движением", "interactive-dog-ball", 42, "Интерактивный мяч для собак", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3150) },
                    { new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3176), "Надёжный нейлоновый поводок для крупных собак.", true, 1490m, 1290m, 4.7m, 87, "Прочный поводок", "xl-nylon-leash", 33, "Поводок нейлоновый XL", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3176) },
                    { new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3198), "Удобная щётка для удаления выпавшей шерсти.", true, 790m, 590m, 4.9m, 128, "Щётка с самоочисткой", "self-cleaning-cat-brush", 68, "Самоочищающаяся щетка для кошек", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3198) },
                    { new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3169), "Прочный канат для игр с собакой.", true, 890m, 690m, 4.7m, 98, "Крепкий канат", "rope-tug-toy", 53, "Канатная игрушка для перетягивания", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3169) },
                    { new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3266), "Пробиотический комплекс для ЖКТ.", true, 1390m, 1190m, 4.6m, 88, "Нормализация микрофлоры", "probiotics-pet", 29, "Пробиотики для пищеварения", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3266) },
                    { new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3252), "Полный набор для ухода за зубами.", true, 1490m, 1290m, 4.8m, 109, "Гигиена полости рта", "pet-dental-kit", 37, "Набор для чистки зубов питомцам", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3252) },
                    { new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3194), "Профессиональный когтерез с ограничителем.", true, null, 690m, 4.4m, 73, "Безопасный когтерез", "pet-nail-clipper", 55, "Когтерез для животных", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3195) },
                    { new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3248), "Быстрое очищение шерсти без купания.", true, null, 890m, 4.5m, 64, "Сухой шампунь без воды", "dry-dog-shampoo", 82, "Сухой шампунь для собак", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3249) },
                    { new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3260), "Капли на холку от блох и клещей.", true, null, 890m, 4.9m, 201, "Защита от паразитов", "anti-parasite-drops-cats", 73, "Антипаразитарные капли для кошек", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3260) },
                    { new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3132), "Нежные кусочки в соусе. Идеальный рацион для взрослых кошек.", true, null, 89m, 4.7m, 189, "Влажный корм для кошек", "whiskas-wet-chicken", 320, "Whiskas Wet Cat Food с курицей", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3132) },
                    { new Guid("9dac461d-d71a-4420-967f-dc397994f358"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3165), "Классическая лазерная указка.", true, null, 390m, 4.8m, 245, "Лазер для активных игр", "cat-laser-pointer", 180, "Лазерная указка для кошек", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3166) },
                    { new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3172), "Мягкая и удобная шлейка для средних и крупных собак.", true, null, 1690m, 4.8m, 112, "Комфортная шлейка", "adjustable-dog-harness", 28, "Регулируемая шлейка для собак", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3172) },
                    { new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3136), "Сбалансированное питание для правильного роста и развития щенков.", true, 2190m, 1890m, 4.8m, 98, "Корм для щенков всех пород", "pedigree-puppy", 64, "Pedigree Puppy для щенков", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3137) },
                    { new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3191), "Натуральный шампунь с экстрактом алоэ.", true, 990m, 790m, 4.6m, 95, "Гипоаллергенный шампунь", "dog-shampoo-aloe", 47, "Шампунь для собак с алоэ", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3192) },
                    { new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3153), "Лёгкая удочка с яркими перьями.", true, null, 490m, 4.9m, 312, "Любимая игрушка кошек", "cat-feather-wand", 95, "Дразнилка с перьями для кошек", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3154) },
                    { new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3162), "Прочный плюш с пищалкой внутри.", true, 1190m, 890m, 4.5m, 134, "Мягкая пищащая игрушка", "squeaky-plush-bear", 67, "Пищащий плюшевый медведь", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3162) },
                    { new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3141), "Гипоаллергенный корм для собак с чувствительным пищеварением.", true, null, 3190m, 4.9m, 156, "Беззерновой корм с лососем", "brit-care-salmon", 45, "Brit Care Grain-Free Salmon", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3141) },
                    { new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3182), "Керамическая миска с резиновым основанием.", true, null, 790m, 4.6m, 203, "Устойчивая миска", "ceramic-non-slip-bowl", 124, "Миска керамическая антискользящая", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3183) },
                    { new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3270), "Добавка для правильного формирования скелета.", true, null, 690m, 4.8m, 67, "Кальций для роста костей", "calcium-vitamin-d", 95, "Кальций с витамином D для щенков", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3270) },
                    { new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3145), "Полезные косточки и палочки для поощрения.", true, 590m, 450m, 4.6m, 87, "Натуральные лакомства", "bosch-dog-treats", 210, "Лакомство для собак Bosch", new DateTime(2026, 4, 11, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3145) }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "Alt", "ProductId", "Url" },
                values: new object[,]
                {
                    { new Guid("061053b7-f878-4dbb-afca-d305e9abc210"), "Ceramic Non-Slip Bowl", new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("08a8fa90-bce7-4ed9-9f34-9eff534817f6"), "Pedigree Puppy Food", new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), "https://picsum.photos/id/106/800/800" },
                    { new Guid("0a27abb0-6860-4da7-a1aa-8437b6d9a635"), "XL Dog Leash", new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("0c527aec-7be0-4c8a-ac6c-4eef7a041ecb"), "Adjustable Dog Harness", new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("147eed91-5c88-4b4e-af56-bbb388d70840"), "Cat Laser Pointer", new Guid("9dac461d-d71a-4420-967f-dc397994f358"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("1cf2ce5f-1044-4193-89d1-7e3560714d5c"), "Anti-Parasite Drops", new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("2a7ddade-bed3-4607-abd8-f89a1228cfed"), "Pet Dental Kit", new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("43443ae0-e95b-43aa-a452-0d7dfaf728a0"), "Calcium with Vitamin D", new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("58d65ca7-1edc-4caa-8b4a-a9e41ea0881a"), "Probiotics", new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("62c99431-90e1-4189-ba92-8f61d7e2c074"), "Joint Supplement", new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("63fd849b-949b-46a0-bad3-21e0576f42aa"), "Brit Care Grain-Free Salmon", new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("6b9d0e93-82fe-48ce-a3c5-a59075e4f502"), "Pet Nail Clipper", new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("7069ec83-43e8-471f-b72e-847b8165c3f4"), "Cat Feather Wand Toy", new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), "https://picsum.photos/id/133/800/800" },
                    { new Guid("75b584f9-f586-4471-aaed-63cbaef9431f"), "Interactive Dog Ball", new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("7ebc623a-0a71-42ed-8690-82b612185aa5"), "Dry Dog Shampoo", new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("80aa83de-c471-451d-8573-38cc96512807"), "Warm Dog Jumpsuit", new Guid("3080a42d-ab57-414e-8925-82775c1de349"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("870e4897-a009-473b-b61d-a2ce2960adda"), "Dog Shampoo", new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("91f5fda3-385e-482e-95ea-88fdff77a417"), "GPS Pet Collar", new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("a34a10e8-e3cf-4699-86fe-12d293f03c64"), "Rope Tug Toy", new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("b1648ab6-3b33-4ee8-aea9-c5801c390362"), "Bosch Dog Treats", new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("c0622040-7e6c-4cd1-9bed-9f6fe84c202f"), "Royal Canin Adult Dog Food", new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), "https://picsum.photos/id/237/800/800" },
                    { new Guid("c3d397c9-1446-425c-94b9-1e338f79ca7b"), "Whiskas Wet Cat Food", new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), "https://picsum.photos/id/1015/800/800" },
                    { new Guid("cf8c1216-2660-4966-a673-5feba3690e15"), "Self-Cleaning Cat Brush", new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("d2c838f6-f2d7-4069-a59a-d730c970e04a"), "Squeaky Plush Bear", new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), "https://picsum.photos/id/367/800/800" }
                });

            migrationBuilder.InsertData(
                table: "ProductReviews",
                columns: new[] { "Id", "AuthorName", "CreatedAt", "ProductId", "Rating", "Text" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "Анна Смирнова", new DateTime(2026, 2, 25, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3422), new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), 5, "Отличный корм! Собака ест с огромным удовольствием, шерсть стала намного лучше." },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), "Дмитрий Волков", new DateTime(2026, 3, 22, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3427), new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), 5, "Лучшая игрушка за последнее время! Собака не может остановиться." }
                });

            migrationBuilder.InsertData(
                table: "ProductReviewReplys",
                columns: new[] { "Id", "AuthorName", "CreatedAt", "ReviewId", "Text" },
                values: new object[,]
                {
                    { new Guid("5873dcb6-0a40-4028-8430-8b2a0e9dbc80"), "Tailly Shop", new DateTime(2026, 2, 26, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3446), new Guid("a1111111-1111-1111-1111-111111111111"), "Спасибо большое за отзыв, Анна! Очень рады, что вашему питомцу нравится наш корм ❤️" },
                    { new Guid("dbd50f66-98e0-4354-857d-07b9badb1cab"), "Tailly Shop", new DateTime(2026, 3, 23, 10, 10, 55, 494, DateTimeKind.Utc).AddTicks(3448), new Guid("a3333333-3333-3333-3333-333333333333"), "Рады, что игрушка понравилась! Это один из наших бестселлеров." }
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
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");

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
