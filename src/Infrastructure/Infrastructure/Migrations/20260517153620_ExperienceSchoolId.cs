using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExperienceSchoolId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Experiences");

            migrationBuilder.Sql("DELETE FROM Experiences");

            migrationBuilder.AddColumn<long>(
                name: "SchoolId",
                table: "Experiences",
                type: "bigint",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_SchoolId",
                table: "Experiences",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiences_Schools_SchoolId",
                table: "Experiences",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiences_Schools_SchoolId",
                table: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_SchoolId",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Experiences");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Experiences",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
