using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    Sound = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PositionCode = table.Column<int>(type: "int", nullable: false),
                    GameType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameProgresses_ChildId_Sound_PositionCode_GameType",
                table: "GameProgresses",
                columns: new[] { "ChildId", "Sound", "PositionCode", "GameType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameProgresses");
        }
    }
}
