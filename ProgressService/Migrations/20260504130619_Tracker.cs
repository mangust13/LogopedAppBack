using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressService.Migrations
{
    /// <inheritdoc />
    public partial class Tracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyActivities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_ChildId_Date_ActivityType",
                table: "DailyActivities",
                columns: new[] { "ChildId", "Date", "ActivityType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyActivities");
        }
    }
}
