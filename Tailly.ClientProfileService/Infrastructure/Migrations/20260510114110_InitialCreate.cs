using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tailly.ClientProfileService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    AgeYears = table.Column<int>(type: "integer", nullable: false),
                    AgeMonths = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    ToOtherPets = table.Column<int>(type: "integer", nullable: true),
                    ToKidsUnder10 = table.Column<int>(type: "integer", nullable: true),
                    StaysHomeAlone = table.Column<int>(type: "integer", nullable: true),
                    Vaccinated = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BreedId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pets_ClientProfiles_ClientId",
                        column: x => x.ClientId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Breeds",
                columns: new[] { "Id", "Description", "Title", "Type" },
                values: new object[,]
                {
                    { new Guid("0532630b-bbe8-4a9c-93ec-9665e3c583d7"), null, "Корги", 1 },
                    { new Guid("0afc029f-2bac-4b5a-9663-054c089658c6"), null, "Сиамская", 2 },
                    { new Guid("110d4844-2c68-4b28-afcc-e198a6e384b3"), null, "Лев", 5 },
                    { new Guid("1c2e950c-6164-465b-b0ec-0259594c34ae"), null, "Хаски", 1 },
                    { new Guid("37990fe4-6b76-475f-a378-98eea7acbc26"), null, "Французский бульдог", 1 },
                    { new Guid("4dd3be4e-dc09-45bd-b12e-1ed59fbd90bd"), null, "Британская короткошерстная", 2 },
                    { new Guid("75a77134-6b6f-40e1-a642-a3a4cd0b2fce"), null, "Немецкая овчарка", 1 },
                    { new Guid("7c19145a-7818-4beb-8800-f3cff753eb60"), null, "Рэгдолл", 2 },
                    { new Guid("80596eed-28f8-4961-8fd5-9d0eba0bd06c"), null, "Шотландская вислоухая", 2 },
                    { new Guid("9a0fdece-deb0-43b7-81ef-0d6adb6c82ef"), null, "Баран", 5 },
                    { new Guid("a3815616-d7df-4403-be61-455e9728b2bf"), null, "Корелла", 3 },
                    { new Guid("be045962-4d3a-4cf5-8414-12613419bb2a"), null, "Лабрадор", 1 },
                    { new Guid("d4302ee8-7b5d-4a29-a572-f3587404e6e4"), null, "Мейн-кун", 2 },
                    { new Guid("d44c9777-1298-4cae-9d72-2fcf80af9c19"), null, "Волнистый попугай", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_Type",
                table: "Breeds",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_Type_Title",
                table: "Breeds",
                columns: new[] { "Type", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfiles_CityId",
                table: "ClientProfiles",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfiles_UserId",
                table: "ClientProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_BreedId",
                table: "Pets",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_ClientId",
                table: "Pets",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "ClientProfiles");
        }
    }
}
