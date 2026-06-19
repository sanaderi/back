using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Experience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections");

            migrationBuilder.AddColumn<bool>(
                name: "SubscribeToActivityFeed",
                table: "Connections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ProfileView",
                table: "ApplicationUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiences_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections",
                columns: new[] { "SourceUserId", "DestinationUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_UserId",
                table: "Experiences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "SubscribeToActivityFeed",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "ProfileView",
                table: "ApplicationUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections",
                columns: new[] { "SourceUserId", "DestinationUserId" },
                unique: true);
        }
    }
}
