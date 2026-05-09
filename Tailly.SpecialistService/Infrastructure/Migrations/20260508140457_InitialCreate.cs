using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tailly.SpecialistService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecialistApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    About = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExperienceYears = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    AnimalTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ServiceFormats = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CanGiveMedication = table.Column<bool>(type: "boolean", nullable: false),
                    CanHandleDifficultBehavior = table.Column<bool>(type: "boolean", nullable: false),
                    CanTakeOvernightOrders = table.Column<bool>(type: "boolean", nullable: false),
                    HasOwnPets = table.Column<bool>(type: "boolean", nullable: false),
                    HasPetFirstAidBasics = table.Column<bool>(type: "boolean", nullable: false),
                    HousingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DistrictPreferences = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SchedulePreferences = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PortfolioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Motivation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AdditionalInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewComment = table.Column<string>(type: "text", nullable: true),
                    ReviewedBy = table.Column<string>(type: "text", nullable: true),
                    InterviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InterviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedSpecialistId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedSpecialistSlug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SpecialistAccountCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialistApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specialists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    District = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedOrdersCount = table.Column<int>(type: "integer", nullable: false),
                    RepeatOrdersCount = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timezone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    HousingType = table.Column<int>(type: "integer", nullable: false),
                    HasChildrenUnderTen = table.Column<int>(type: "integer", nullable: false),
                    About = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Details_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReplyText = table.Column<string>(type: "text", nullable: true),
                    ReplyCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Photos = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PriceUnit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialistGalleries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Alt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialistGalleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialistGalleries_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarAvailabilityWindows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarAvailabilityWindows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarAvailabilityWindows_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarBookedSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarBookedSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarBookedSlots_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarBookingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayStartTime = table.Column<string>(type: "text", nullable: false),
                    DayEndTime = table.Column<string>(type: "text", nullable: false),
                    SlotStepMinutes = table.Column<int>(type: "integer", nullable: false),
                    DefaultDurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarBookingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarBookingSettings_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarDayOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDayOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarDayOverrides_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetAges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PetAge = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetAges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetAges_Details_DetailsId",
                        column: x => x.DetailsId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetSizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PetSize = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetSizes_Details_DetailsId",
                        column: x => x.DetailsId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PetType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetTypes_Details_DetailsId",
                        column: x => x.DetailsId,
                        principalTable: "Details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Specialists",
                columns: new[] { "Id", "AvatarUrl", "City", "CompletedOrdersCount", "CreatedAt", "District", "Email", "ExperienceYears", "FirstName", "LastName", "Latitude", "Longitude", "MiddleName", "Phone", "Rating", "RepeatOrdersCount", "ReviewsCount", "Slug", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "/uploads/avatars/anna.jpg", "Москва", 87, new DateTime(2025, 11, 8, 14, 4, 56, 434, DateTimeKind.Utc).AddTicks(5976), "Сокольники", "anna@example.com", 5, "Анна", "Смирнова", 55.793100000000003, 37.677799999999998, null, "+79161234567", 4.8m, 24, 42, "anna-petcare", new Guid("438a4a9e-c17b-439d-ab8b-007ed01d2ac8") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, "Москва", 45, new DateTime(2026, 1, 8, 14, 4, 56, 434, DateTimeKind.Utc).AddTicks(6021), "Марьино", "dima@example.com", 3, "Дмитрий", "Кузнецов", 55.6494, 37.743000000000002, null, "+79162345678", 4.6m, 12, 28, "dima-dogwalker", new Guid("6fbf4dc8-cf8b-46df-960f-b6d0c6eb7ff9") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "Санкт-Петербург", 112, new DateTime(2025, 9, 8, 14, 4, 56, 434, DateTimeKind.Utc).AddTicks(6025), "Центральный", "maria@example.com", 7, "Мария", "Попова", 59.938600000000001, 30.3141, null, "+79213456789", 4.9m, 35, 67, "maria-grooming", new Guid("ad0ec355-8d4f-4e96-9663-d497cb48a47a") }
                });

            migrationBuilder.InsertData(
                table: "Calendars",
                columns: new[] { "Id", "SpecialistId", "Timezone" },
                values: new object[,]
                {
                    { new Guid("44733e3a-29f9-4371-a4be-14d6a5e02b16"), new Guid("33333333-3333-3333-3333-333333333333"), "Europe/Moscow" },
                    { new Guid("c2a498d2-75f5-4846-ad8b-87106e566de4"), new Guid("22222222-2222-2222-2222-222222222222"), "Europe/Moscow" },
                    { new Guid("c6e0fea6-4b86-4001-be26-29800be5c900"), new Guid("11111111-1111-1111-1111-111111111111"), "Europe/Moscow" }
                });

            migrationBuilder.InsertData(
                table: "Details",
                columns: new[] { "Id", "About", "HasChildrenUnderTen", "HousingType", "SpecialistId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Люблю животных, имею двух своих собак. Занимаюсь выгулом и передержкой уже более 5 лет.", 2, 1, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Профессиональный выгульщик с опытом работы с крупными породами.", 1, 2, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Профессиональный грумер собак и кошек с 7-летним стажем.", 3, 1, new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Description", "Name", "Price", "PriceUnit", "SpecialistId" },
                values: new object[,]
                {
                    { new Guid("18803d15-7feb-4ba8-9ec3-7b65418d8de7"), "Выгул собаки 60 минут", 1, 1200m, 4, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("2d9bcfc7-897e-4b89-a3ea-67fb58d146f4"), "Комфортная передержка в квартире", 2, 2500m, 2, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a0a85b66-ac0f-4604-8727-e6dc052cd174"), "Выгул с элементами дрессировки", 5, 1800m, 1, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("fe894bf6-3eb7-4341-b34d-d341e20d2128"), "Полный груминг", 3, 3500m, 3, new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "SpecialistGalleries",
                columns: new[] { "Id", "Alt", "ImageUrl", "Order", "SpecialistId" },
                values: new object[,]
                {
                    { new Guid("0e28d891-7e5c-4caa-ad41-d5a44c24df29"), "Анна на прогулке", "/uploads/gallery/anna2.jpg", 2, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("4c797f51-5100-4a92-9b5b-67ec2498dc94"), "Анна с питомцем", "/uploads/gallery/anna1.jpg", 1, new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "PetAges",
                columns: new[] { "Id", "DetailsId", "PetAge" },
                values: new object[,]
                {
                    { new Guid("24cf62f2-ecec-4b3f-b806-38ba28b6fc78"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 3 },
                    { new Guid("5934c383-839d-4a8c-8392-feefd2e1ab57"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 4 }
                });

            migrationBuilder.InsertData(
                table: "PetSizes",
                columns: new[] { "Id", "DetailsId", "PetSize" },
                values: new object[,]
                {
                    { new Guid("4b4854ad-b0b6-4e72-9439-f5f601f7b9b8"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4 },
                    { new Guid("7abf6d66-d2db-4e2f-b9d8-023e0246da5a"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 3 },
                    { new Guid("89833d44-ea5f-423d-b19b-9c2e1eb9783b"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 2 },
                    { new Guid("d3d93b21-7e7e-45e3-a5f1-15e5f707dffd"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5 }
                });

            migrationBuilder.InsertData(
                table: "PetTypes",
                columns: new[] { "Id", "DetailsId", "PetType" },
                values: new object[,]
                {
                    { new Guid("11150932-055f-4f0d-97e8-12c88e1179e6"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 1 },
                    { new Guid("3aa32ec3-f085-4337-a9fe-409e34b63245"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1 },
                    { new Guid("65b9f374-92bc-4d00-9b77-0ba77916ff91"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 1 },
                    { new Guid("ac496664-f005-4d33-ae0e-d05bf2adf112"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 2 },
                    { new Guid("f2cc3f03-70f2-4524-b7a3-62f29eb1dcf7"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarAvailabilityWindows_CalendarId_Date",
                table: "CalendarAvailabilityWindows",
                columns: new[] { "CalendarId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarBookedSlots_CalendarId_Date",
                table: "CalendarBookedSlots",
                columns: new[] { "CalendarId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarBookingSettings_CalendarId",
                table: "CalendarBookingSettings",
                column: "CalendarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDayOverrides_CalendarId_Date",
                table: "CalendarDayOverrides",
                columns: new[] { "CalendarId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_SpecialistId",
                table: "Calendars",
                column: "SpecialistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Details_SpecialistId",
                table: "Details",
                column: "SpecialistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetAges_DetailsId",
                table: "PetAges",
                column: "DetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PetSizes_DetailsId",
                table: "PetSizes",
                column: "DetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PetTypes_DetailsId",
                table: "PetTypes",
                column: "DetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SpecialistId",
                table: "Reviews",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_SpecialistId",
                table: "Services",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistApplications_CreatedAt",
                table: "SpecialistApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistApplications_Email",
                table: "SpecialistApplications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistApplications_Status",
                table: "SpecialistApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistApplications_UserId",
                table: "SpecialistApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistGalleries_SpecialistId_Order",
                table: "SpecialistGalleries",
                columns: new[] { "SpecialistId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Specialists_Slug",
                table: "Specialists",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarAvailabilityWindows");

            migrationBuilder.DropTable(
                name: "CalendarBookedSlots");

            migrationBuilder.DropTable(
                name: "CalendarBookingSettings");

            migrationBuilder.DropTable(
                name: "CalendarDayOverrides");

            migrationBuilder.DropTable(
                name: "PetAges");

            migrationBuilder.DropTable(
                name: "PetSizes");

            migrationBuilder.DropTable(
                name: "PetTypes");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "SpecialistApplications");

            migrationBuilder.DropTable(
                name: "SpecialistGalleries");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.DropTable(
                name: "Specialists");
        }
    }
}
