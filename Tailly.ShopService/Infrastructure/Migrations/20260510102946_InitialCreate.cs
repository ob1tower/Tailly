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
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ForWhom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Purpose = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PetSize = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Material = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    PickupPointId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionEmailSent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                name: "ProductReviewImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviewImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviewImages_ProductReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ProductReviews",
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
                    { new Guid("0be164bd-10d3-4975-81ac-254b49fafe9d"), "Казань, ул. Баумана, 27", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6375), 1, "ПВЗ СДЭК на Баумана" },
                    { new Guid("34d8aa41-aca8-4290-b493-03605c4f0c7c"), "Казань, пр. Победы, 74", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6376), 1, "ПВЗ СДЭК в Советском районе" },
                    { new Guid("440e5231-7206-4f6e-bbf5-283ba66d73f4"), "Москва, проспект Мира, 119, стр. 23", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6359), 1, "ПВЗ СДЭК на ВДНХ" },
                    { new Guid("7128b4a2-f5ce-4a6d-b6b2-0e8fa3ca7baf"), "Москва, Ленинский проспект, 45", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6356), 1, "ПВЗ СДЭК на Ленинском проспекте" },
                    { new Guid("79bdfa43-3f64-4ce3-9f12-4b187393f3e9"), "Москва, г. Химки, ул. Ленинградская, 29", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6357), 1, "ПВЗ СДЭК в Химках" },
                    { new Guid("84cb09a5-1776-4834-bb7a-683ea8b9c1f4"), "Санкт-Петербург, пр. Испытателей, 15", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6367), 1, "ПВЗ СДЭК у станции метро Пионерская" },
                    { new Guid("a112ef56-9db3-4445-951b-199772cad332"), "Санкт-Петербург, Невский проспект, 56", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6360), 1, "ПВЗ СДЭК на Невском" },
                    { new Guid("a5867f95-8a76-45cd-95a0-4c3ec06f7362"), "Екатеринбург, ул. Космонавтов, 11/1", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6370), 1, "ПВЗ СДЭК в Уралмаше" },
                    { new Guid("b9180c49-1885-4f0c-8db3-00ecabf01494"), "Санкт-Петербург, ул. Будапештская, 18", new DateTime(2026, 5, 12, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6362), 1, "ПВЗ СДЭК в Купчино" },
                    { new Guid("bc6f69ec-d710-4860-8ac6-27b6e1a8efbb"), "Екатеринбург, ул. Малышева, 51", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6368), 1, "ПВЗ СДЭК на Малышева" },
                    { new Guid("c8a2ebd1-2799-4f19-a8d2-4cb6bfef4782"), "Москва, ул. Тверская, 12, стр. 1", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6349), 1, "ПВЗ СДЭК на Тверской" },
                    { new Guid("cc8f568f-24f7-4599-b674-6646a1c7c469"), "Новосибирск, Красный проспект, 52", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6371), 1, "ПВЗ СДЭК на Красном проспекте" },
                    { new Guid("e32baa7e-13d1-4d4f-ae5d-6dd240c9c72e"), "Новосибирск, ул. Ильича, 10", new DateTime(2026, 5, 13, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6373), 1, "ПВЗ СДЭК в Академгородке" },
                    { new Guid("edbd7e63-a832-4abf-a5c7-59e0270ea53c"), "Санкт-Петербург, Московский проспект, 183", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6365), 1, "ПВЗ СДЭК на Московском проспекте" },
                    { new Guid("f20f7832-3493-4b6d-95d3-8232d975aa0b"), "Москва, ул. Братиславская, 21, корп. 2", new DateTime(2026, 5, 11, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6354), 1, "ПВЗ СДЭК в Марьино" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CountryOfOrigin", "CreatedAt", "Description", "ForWhom", "IsAvailable", "Material", "OldPrice", "PetSize", "Price", "Purpose", "Rating", "ReviewsCount", "ShortDescription", "Slug", "StockQuantity", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), "JointVet", new Guid("55555555-5555-5555-5555-555555555555"), "Германия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6181), "Комплекс для укрепления суставов и хрящей.", "Собаки", true, "Таблетки", 1790m, "Средний/Крупный", 1490m, "Здоровье суставов", 0m, 0, "Поддержка суставов", "joint-supplement-dogs", 51, "Витамины для суставов собак", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6182) },
                    { new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), "Royal Canin", new Guid("11111111-1111-1111-1111-111111111111"), "Франция", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6018), "Премиальный полнорационный корм для собак старше 1 года. Поддерживает иммунитет и здоровье пищеварения.", "Собаки", true, "Сухой корм", 2790m, "Все размеры", 2490m, "Ежедневное питание", 0m, 0, "Сухой корм для взрослых собак", "royal-canin-adult-dog", 87, "Royal Canin Adult Dog Food", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6018) },
                    { new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), "SmartTrack", new Guid("33333333-3333-3333-3333-333333333333"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6092), "Ошейник с GPS и отслеживанием активности.", "Собаки", false, "Нейлон", 3990m, "Средний/Крупный", 3490m, "Отслеживание местоположения", 0m, 0, "Умный ошейник", "gps-pet-collar", 12, "Ошейник с GPS-трекером", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6092) },
                    { new Guid("3080a42d-ab57-414e-8925-82775c1de349"), "WinterPet", new Guid("33333333-3333-3333-3333-333333333333"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6082), "Утеплённый комбинезон с капюшоном.", "Собаки", true, "Полиэстер + синтепон", 2590m, "Средний/Крупный", 2190m, "Защита от холода", 0m, 0, "Зимняя одежда для собак", "warm-dog-jumpsuit", 19, "Теплый комбинезон для собак", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6083) },
                    { new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), "SmartPet", new Guid("22222222-2222-2222-2222-222222222222"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6049), "Автоматический мячик с несколькими режимами движения.", "Собаки", true, "Пластик", 1590m, "Средний/Крупный", 1290m, "Активные игры", 0m, 0, "Умная игрушка с движением", "interactive-dog-ball", 42, "Интерактивный мяч для собак", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6050) },
                    { new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), "ProLeash", new Guid("33333333-3333-3333-3333-333333333333"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6078), "Надёжный нейлоновый поводок для крупных собак.", "Собаки", false, "Нейлон", 1490m, "Крупный", 1290m, "Выгул", 0m, 0, "Прочный поводок", "xl-nylon-leash", 33, "Поводок нейлоновый XL", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6078) },
                    { new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), "FurCare", new Guid("44444444-4444-4444-4444-444444444444"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6168), "Удобная щётка для удаления выпавшей шерсти.", "Кошки", true, "Пластик + металл", 790m, "Все размеры", 590m, "Уход за шерстью", 0m, 0, "Щётка с самоочисткой", "self-cleaning-cat-brush", 68, "Самоочищающаяся щетка для кошек", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6168) },
                    { new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), "StrongDog", new Guid("22222222-2222-2222-2222-222222222222"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6069), "Прочный канат для игр с собакой.", "Собаки", true, "Хлопковый канат", 890m, "Средний/Крупный", 690m, "Перетягивание", 0m, 0, "Крепкий канат", "rope-tug-toy", 53, "Канатная игрушка для перетягивания", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6069) },
                    { new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), "BioGut", new Guid("55555555-5555-5555-5555-555555555555"), "Германия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6196), "Пробиотический комплекс для ЖКТ.", "Собаки/Кошки", false, "Порошок", 1390m, "Все размеры", 1190m, "Нормализация пищеварения", 0m, 0, "Нормализация микрофлоры", "probiotics-pet", 29, "Пробиотики для пищеварения", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6196) },
                    { new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), "DentalCare", new Guid("44444444-4444-4444-4444-444444444444"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6177), "Полный набор для ухода за зубами.", "Собаки/Кошки", true, "Пластик + металл", 1490m, "Все размеры", 1290m, "Гигиена зубов", 0m, 0, "Гигиена полости рта", "pet-dental-kit", 37, "Набор для чистки зубов питомцам", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6177) },
                    { new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), "GroomPro", new Guid("44444444-4444-4444-4444-444444444444"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6163), "Профессиональный когтерез с ограничителем.", "Собаки/Кошки", true, "Нержавеющая сталь", null, "Все размеры", 690m, "Уход за когтями", 0m, 0, "Безопасный когтерез", "pet-nail-clipper", 55, "Когтерез для животных", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6163) },
                    { new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), "CleanPaw", new Guid("44444444-4444-4444-4444-444444444444"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6172), "Быстрое очищение шерсти без купания.", "Собаки", true, "Сухой порошок", null, "Все размеры", 890m, "Сухое очищение", 0m, 0, "Сухой шампунь без воды", "dry-dog-shampoo", 82, "Сухой шампунь для собак", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6172) },
                    { new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), "ParasiteGuard", new Guid("55555555-5555-5555-5555-555555555555"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6186), "Капли на холку от блох и клещей.", "Кошки", true, "Капли", null, "Все размеры", 890m, "Защита от паразитов", 0m, 0, "Защита от паразитов", "anti-parasite-drops-cats", 73, "Антипаразитарные капли для кошек", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6186) },
                    { new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), "Whiskas", new Guid("11111111-1111-1111-1111-111111111111"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6024), "Нежные кусочки в соусе. Идеальный рацион для взрослых кошек.", "Кошки", true, "Влажный корм", null, "Все размеры", 89m, "Ежедневное питание", 0m, 0, "Влажный корм для кошек", "whiskas-wet-chicken", 320, "Whiskas Wet Cat Food с курицей", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6024) },
                    { new Guid("9dac461d-d71a-4420-967f-dc397994f358"), "LaserPet", new Guid("22222222-2222-2222-2222-222222222222"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6064), "Классическая лазерная указка.", "Кошки", true, "Пластик", null, "Все размеры", 390m, "Физическая и умственная нагрузка", 0m, 0, "Лазер для активных игр", "cat-laser-pointer", 180, "Лазерная указка для кошек", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6064) },
                    { new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), "ComfortWalk", new Guid("33333333-3333-3333-3333-333333333333"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6073), "Мягкая и удобная шлейка для средних и крупных собак.", "Собаки", true, "Нейлон + флис", null, "Средний/Крупный", 1690m, "Выгул", 0m, 0, "Комфортная шлейка", "adjustable-dog-harness", 28, "Регулируемая шлейка для собак", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6073) },
                    { new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), "Pedigree", new Guid("11111111-1111-1111-1111-111111111111"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6029), "Сбалансированное питание для правильного роста и развития щенков.", "Щенки", true, "Сухой корм", 2190m, "Все размеры", 1890m, "Рост и развитие", 0m, 0, "Корм для щенков всех пород", "pedigree-puppy", 64, "Pedigree Puppy для щенков", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6029) },
                    { new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), "PurePet", new Guid("44444444-4444-4444-4444-444444444444"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6158), "Натуральный шампунь с экстрактом алоэ.", "Собаки", false, "Жидкий шампунь", 990m, "Все размеры", 790m, "Гигиена шерсти", 0m, 0, "Гипоаллергенный шампунь", "dog-shampoo-aloe", 47, "Шампунь для собак с алоэ", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6158) },
                    { new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), "PlayCat", new Guid("22222222-2222-2222-2222-222222222222"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6054), "Лёгкая удочка с яркими перьями.", "Кошки", true, "Перья + пластик", null, "Все размеры", 490m, "Охота и активность", 0m, 0, "Любимая игрушка кошек", "cat-feather-wand", 95, "Дразнилка с перьями для кошек", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6055) },
                    { new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), "FunToys", new Guid("22222222-2222-2222-2222-222222222222"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6059), "Прочный плюш с пищалкой внутри.", "Собаки", true, "Плюш", 1190m, "Маленький/Средний", 890m, "Перетягивание и жевание", 0m, 0, "Мягкая пищащая игрушка", "squeaky-plush-bear", 67, "Пищащий плюшевый медведь", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6060) },
                    { new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), "Brit Care", new Guid("11111111-1111-1111-1111-111111111111"), "Чехия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6040), "Гипоаллергенный корм для собак с чувствительным пищеварением.", "Собаки", true, "Сухой корм", null, "Все размеры", 3190m, "Гипоаллергенное питание", 0m, 0, "Беззерновой корм с лососем", "brit-care-salmon", 45, "Brit Care Grain-Free Salmon", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6040) },
                    { new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), "CeramicPet", new Guid("33333333-3333-3333-3333-333333333333"), "Китай", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6087), "Керамическая миска с резиновым основанием.", "Собаки/Кошки", true, "Керамика + резина", null, "Все размеры", 790m, "Кормление", 0m, 0, "Устойчивая миска", "ceramic-non-slip-bowl", 124, "Миска керамическая антискользящая", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6087) },
                    { new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), "BoneGrow", new Guid("55555555-5555-5555-5555-555555555555"), "Россия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6200), "Добавка для правильного формирования скелета.", "Щенки", true, "Таблетки", null, "Маленький/Средний", 690m, "Рост костей и зубов", 0m, 0, "Кальций для роста костей", "calcium-vitamin-d", 95, "Кальций с витамином D для щенков", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6200) },
                    { new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), "Bosch", new Guid("11111111-1111-1111-1111-111111111111"), "Германия", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6044), "Полезные косточки и палочки для поощрения.", "Собаки", true, "Лакомства", 590m, "Все размеры", 450m, "Поощрение и тренировка", 0m, 0, "Натуральные лакомства", "bosch-dog-treats", 210, "Лакомство для собак Bosch", new DateTime(2026, 5, 10, 10, 29, 46, 321, DateTimeKind.Utc).AddTicks(6045) }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "Alt", "ProductId", "Url" },
                values: new object[,]
                {
                    { new Guid("0acef4f6-1f49-4c9b-b060-a1dfcd8d1a6b"), "Squeaky Plush Bear", new Guid("cbc1ac00-2283-45d3-b33e-0a7f1373ae2c"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("23749e10-3ffb-47e7-9f05-acab4ccb1220"), "GPS Pet Collar", new Guid("23746c5a-0e90-4e6a-b902-445f92756b98"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("30ec262f-fdb5-43b7-a33a-97154b8c4375"), "Brit Care Grain-Free Salmon", new Guid("d7a9090a-6d01-4a36-9e9d-2fd9fef935a4"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("4af4c958-11f2-4a3a-b207-6e142d3b4ed4"), "Joint Supplement", new Guid("065f9a01-157f-4c29-9743-c9b1a562562d"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("507f9145-f9e9-423e-8040-1ba9425b81c9"), "Cat Laser Pointer", new Guid("9dac461d-d71a-4420-967f-dc397994f358"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("638775b9-c756-40e6-a9a4-5f194ea2f9c1"), "Adjustable Dog Harness", new Guid("a3746572-16ff-4cf3-af67-0b8673bffcf7"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("6c02e20a-62ac-4eba-8f95-edd7e158e583"), "Ceramic Non-Slip Bowl", new Guid("ea88eab3-06a9-4614-80f9-dd8d457a498d"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("75ae3540-0c66-4a2e-9522-a10973f19822"), "Interactive Dog Ball", new Guid("3461d3bf-6549-4049-beb0-4651ab3a4f62"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("7acf1837-b16d-4bfe-bf7d-22beddd798a9"), "XL Dog Leash", new Guid("37083c6d-2711-44de-90a0-b0bf2684dcba"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("7d878183-83f1-46a6-af55-8bdcaa742dfa"), "Pet Nail Clipper", new Guid("6b729cb8-c2dd-4ccf-89b3-05eed1d16c9b"), "https://picsum.photos/id/367/800/800" },
                    { new Guid("7ea300ee-8133-4df3-b553-6fe83d4c6c08"), "Cat Feather Wand Toy", new Guid("c7b91735-1896-4dae-8b03-cfe3b90813f0"), "https://picsum.photos/id/133/800/800" },
                    { new Guid("8014ec72-2be3-48b7-90ee-6e7b2a7e83a1"), "Dog Shampoo", new Guid("ba872d50-5c81-40d4-954e-0ecdb0916efb"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("874fa1ac-7b33-42b1-a26e-39d8328aa2f2"), "Self-Cleaning Cat Brush", new Guid("3be4a5fa-fd45-453d-bbf9-1f314b747ff2"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("87cbe0c4-7ec2-46f5-9cf4-82935cf750b6"), "Bosch Dog Treats", new Guid("f2539156-ab5d-4f0a-b542-dc9cc37c3239"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("888a71ca-89a2-46db-95e4-eb673e0423b3"), "Royal Canin Adult Dog Food", new Guid("1b3647bc-92bd-483f-9aa4-d20130a619a9"), "https://picsum.photos/id/237/800/800" },
                    { new Guid("8c00c78e-56d3-44fe-a267-378159ffe89c"), "Probiotics", new Guid("6084749d-cc5a-4f5e-b83e-553b383c2091"), "https://picsum.photos/id/433/800/800" },
                    { new Guid("94c256b0-bf91-48e3-9002-1bcaa776667b"), "Calcium with Vitamin D", new Guid("ee3b9279-da4b-421e-ac3e-b5b7ae287463"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("96fe18f1-4fd5-4072-b32a-e229cefc65fe"), "Rope Tug Toy", new Guid("538fe373-3572-4dd1-b068-f923543f77d9"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("ab8db22f-cc4a-4dd8-ad19-43ee6acc3d90"), "Pet Dental Kit", new Guid("60a0315b-ebe9-46e4-b56e-acae29a356b6"), "https://picsum.photos/id/201/800/800" },
                    { new Guid("b7251671-9507-427e-bf18-48d7f1b746e8"), "Whiskas Wet Cat Food", new Guid("81acec04-ac74-4f06-861a-261b64eaae85"), "https://picsum.photos/id/1015/800/800" },
                    { new Guid("e3463a51-de06-4387-90d4-14d334fff245"), "Warm Dog Jumpsuit", new Guid("3080a42d-ab57-414e-8925-82775c1de349"), "https://picsum.photos/id/251/800/800" },
                    { new Guid("e56ba742-fdb3-4425-9a08-b85dde303b78"), "Dry Dog Shampoo", new Guid("6d861eaf-a99e-4342-bbe2-c1529376bf06"), "https://picsum.photos/id/180/800/800" },
                    { new Guid("ead41549-3889-4046-8bc4-64ea875b921a"), "Pedigree Puppy Food", new Guid("a9ebd856-d30c-4f8b-8ed2-3ec918a0f865"), "https://picsum.photos/id/106/800/800" },
                    { new Guid("edc88440-fe02-4232-bee6-17e8cd17013c"), "Anti-Parasite Drops", new Guid("73db2a59-d927-4060-ab4a-a71a4ebd7f80"), "https://picsum.photos/id/367/800/800" }
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
                name: "IX_ProductReviewImages_ReviewId",
                table: "ProductReviewImages",
                column: "ReviewId");

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
                name: "ProductReviewImages");

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
