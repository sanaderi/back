using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LastLoginDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoginDate",
                table: "ApplicationUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "ApplicationUsers");
        }
    }
}
