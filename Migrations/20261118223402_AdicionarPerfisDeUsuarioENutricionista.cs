using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace br.com.FitNutri.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPerfisDeUsuarioENutricionista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NutritionistProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CrnRegion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CrnNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OtherSpecialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Presentation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionistProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_NutritionistProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Goal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrainingRoutine = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DietaryRestrictions = table.Column<List<string>>(type: "text[]", nullable: false),
                    OtherDietaryRestrictions = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NutritionistProfiles");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
