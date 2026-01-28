using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddChildAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChildAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChildProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    LogopedUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildAssignments_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildAssignments_Users_LogopedUserId",
                        column: x => x.LogopedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildAssignments_ChildProfileId",
                table: "ChildAssignments",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildAssignments_LogopedUserId",
                table: "ChildAssignments",
                column: "LogopedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildAssignments");
        }
    }
}
