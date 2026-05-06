using System;
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
                    ReplyCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    { new Guid("11111111-1111-1111-1111-111111111111"), "/uploads/avatars/anna.jpg", "Москва", 87, new DateTime(2025, 11, 6, 17, 7, 49, 523, DateTimeKind.Utc).AddTicks(752), "Сокольники", "anna@example.com", 5, "Анна", "Смирнова", 55.793100000000003, 37.677799999999998, null, "+79161234567", 4.8m, 24, 42, "anna-petcare", new Guid("438a4a9e-c17b-439d-ab8b-007ed01d2ac8") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, "Москва", 45, new DateTime(2026, 1, 6, 17, 7, 49, 523, DateTimeKind.Utc).AddTicks(794), "Марьино", "dima@example.com", 3, "Дмитрий", "Кузнецов", 55.6494, 37.743000000000002, null, "+79162345678", 4.6m, 12, 28, "dima-dogwalker", new Guid("bcbd93af-e24d-432e-9c48-86d0103bbf28") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "Санкт-Петербург", 112, new DateTime(2025, 9, 6, 17, 7, 49, 523, DateTimeKind.Utc).AddTicks(810), "Центральный", "maria@example.com", 7, "Мария", "Попова", 59.938600000000001, 30.3141, null, "+79213456789", 4.9m, 35, 67, "maria-grooming", new Guid("2b80396d-b879-4abf-97ff-8b53ce4ccf22") }
                });

            migrationBuilder.InsertData(
                table: "Calendars",
                columns: new[] { "Id", "SpecialistId", "Timezone" },
                values: new object[,]
                {
                    { new Guid("404a5b56-ca67-4990-aa5e-f85518ad5fda"), new Guid("11111111-1111-1111-1111-111111111111"), "Europe/Moscow" },
                    { new Guid("d6f8e041-4b55-4507-a4b5-f883c5ff1814"), new Guid("33333333-3333-3333-3333-333333333333"), "Europe/Moscow" },
                    { new Guid("e4a2f917-2f72-4c75-a32b-873bd451a680"), new Guid("22222222-2222-2222-2222-222222222222"), "Europe/Moscow" }
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
                    { new Guid("0d494aa3-066c-4c79-ba5f-ba16b5b0dc62"), "Выгул с элементами дрессировки", 5, 1800m, 1, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("301a4d2e-af0e-42f1-964b-26d3bb53042c"), "Комфортная передержка в квартире", 2, 2500m, 2, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("894585c0-f54a-4831-a229-3af97c373065"), "Полный груминг", 3, 3500m, 3, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("9e4dfeba-89fb-472e-8d18-05b3f18426ca"), "Выгул собаки 60 минут", 1, 1200m, 4, new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "SpecialistGalleries",
                columns: new[] { "Id", "Alt", "ImageUrl", "Order", "SpecialistId" },
                values: new object[,]
                {
                    { new Guid("97492b9a-7cdc-4d25-8f56-0c6086fa3264"), "Анна на прогулке", "/uploads/gallery/anna2.jpg", 2, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d891fc8d-1c4f-49d8-8bdf-38a8db07dd1a"), "Анна с питомцем", "/uploads/gallery/anna1.jpg", 1, new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "PetAges",
                columns: new[] { "Id", "DetailsId", "PetAge" },
                values: new object[,]
                {
                    { new Guid("7628afe2-19bd-421b-907f-3d955adec9e4"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 3 },
                    { new Guid("98061f42-aa5f-433a-815a-b3cd3a447d85"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 4 }
                });

            migrationBuilder.InsertData(
                table: "PetSizes",
                columns: new[] { "Id", "DetailsId", "PetSize" },
                values: new object[,]
                {
                    { new Guid("2d505ef4-521e-4be9-a786-13d1b7a9fba6"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5 },
                    { new Guid("872c5a69-22fa-4563-a5c4-1e7714adbd24"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 3 },
                    { new Guid("f18ec021-f366-4f60-b483-dd8b322e642a"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 2 },
                    { new Guid("f727f6da-f5f9-4955-aa30-62d81ca8e9ca"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4 }
                });

            migrationBuilder.InsertData(
                table: "PetTypes",
                columns: new[] { "Id", "DetailsId", "PetType" },
                values: new object[,]
                {
                    { new Guid("17d0351f-b708-41b6-a9fd-dd087c178269"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1 },
                    { new Guid("2a8e6135-9e15-4382-ba73-7982ea699eed"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 2 },
                    { new Guid("9a50e91e-df52-407a-b737-da3ae70f795b"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 1 },
                    { new Guid("aaafa93d-f6b3-44ce-8a15-24a32f31e95f"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 2 },
                    { new Guid("c820aead-cdc1-4524-bbe5-bd5511bdfdb4"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 1 }
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
