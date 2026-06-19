using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContentLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentLocalizations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentId = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<int>(type: "int", nullable: false),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentLocalizations_ApplicationUsers_CreationUserId",
                        column: x => x.CreationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContentLocalizations_ApplicationUsers_LastModifyUserId",
                        column: x => x.LastModifyUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContentLocalizations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentLocalizations_CreationUserId",
                table: "ContentLocalizations",
                column: "CreationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentLocalizations_LanguageId_ContentType_ContentId_Name",
                table: "ContentLocalizations",
                columns: new[] { "LanguageId", "ContentType", "ContentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentLocalizations_LastModifyUserId",
                table: "ContentLocalizations",
                column: "LastModifyUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentLocalizations");
        }
    }
}
