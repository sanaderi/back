using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProfileHandle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Handle",
                table: "ApplicationUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_Handle",
                table: "ApplicationUsers",
                column: "Handle",
                unique: true,
                filter: "([Handle] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_Handle",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "Handle",
                table: "ApplicationUsers");
        }
    }
}
