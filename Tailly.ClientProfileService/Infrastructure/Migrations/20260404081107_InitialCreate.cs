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
                    { new Guid("06febfb9-8c1e-473f-a4f8-9180fcd9f7bb"), null, "Лев", 5 },
                    { new Guid("157aa2cd-7cca-4653-acf5-30d2d3b31019"), null, "Рэгдолл", 2 },
                    { new Guid("1e05f6b1-a905-4b97-9aec-858904573f36"), null, "Корелла", 3 },
                    { new Guid("2b415f4b-9698-45e5-b582-d67c9e7e88cf"), null, "Волнистый попугай", 3 },
                    { new Guid("2d47440e-69ff-4364-b5ba-9ecd604255b9"), null, "Баран", 5 },
                    { new Guid("3d604913-3ade-44aa-bb3f-3b7660a84ade"), null, "Шотландская вислоухая", 2 },
                    { new Guid("78df31ee-dec8-4ef1-af30-78f256c5a09b"), null, "Корги", 1 },
                    { new Guid("7d17f57c-2fce-4038-9932-e5a0e799203b"), null, "Хаски", 1 },
                    { new Guid("93faa429-0ad0-404d-b023-09a72a797d6b"), null, "Мейн-кун", 2 },
                    { new Guid("a6ec99c7-be94-4046-8531-d8e6578faaf9"), null, "Немецкая овчарка", 1 },
                    { new Guid("bf34adf6-b6b8-4d7b-8ad7-059e61779959"), null, "Лабрадор", 1 },
                    { new Guid("d769028b-25cb-4fce-b62e-f56f9522181a"), null, "Французский бульдог", 1 },
                    { new Guid("f660d9e9-5450-43b5-a86a-dbb07d51d113"), null, "Британская короткошерстная", 2 },
                    { new Guid("f70bbcf4-5791-4d6f-9121-5732c2ca31b1"), null, "Сиамская", 2 }
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
