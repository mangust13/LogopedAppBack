using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressService.Migrations
{
    /// <inheritdoc />
    public partial class LogopedSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogopedSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogopedId = table.Column<int>(type: "int", nullable: false),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SoundsWorkedOn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogopedSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogopedSessions_ChildId",
                table: "LogopedSessions",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_LogopedSessions_LogopedId",
                table: "LogopedSessions",
                column: "LogopedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogopedSessions");
        }
    }
}
