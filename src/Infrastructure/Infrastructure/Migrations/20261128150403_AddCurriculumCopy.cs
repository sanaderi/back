using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumCopy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeoName",
                table: "CurriculumSeriesTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurriculumCopyIssues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<long>(type: "bigint", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumCopyIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumResourceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumResourceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolQualifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<long>(type: "bigint", nullable: false),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: true),
                    LegacySchoolBoardId = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolQualifications_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SchoolQualifications_CurriculumQualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "CurriculumQualifications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SchoolQualifications_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumCopyIssues_Kind_SourceId",
                table: "CurriculumCopyIssues",
                columns: new[] { "Kind", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumResourceTypes_LegacyId",
                table: "CurriculumResourceTypes",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumResourceTypes_Slug",
                table: "CurriculumResourceTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolQualifications_BoardId_QualificationId",
                table: "SchoolQualifications",
                columns: new[] { "BoardId", "QualificationId" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolQualifications_LegacySchoolBoardId",
                table: "SchoolQualifications",
                column: "LegacySchoolBoardId",
                unique: true,
                filter: "[LegacySchoolBoardId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolQualifications_QualificationId",
                table: "SchoolQualifications",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolQualifications_SchoolId_BoardId_QualificationId",
                table: "SchoolQualifications",
                columns: new[] { "SchoolId", "BoardId", "QualificationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumCopyIssues");

            migrationBuilder.DropTable(
                name: "CurriculumResourceTypes");

            migrationBuilder.DropTable(
                name: "SchoolQualifications");

            migrationBuilder.DropColumn(
                name: "SeoName",
                table: "CurriculumSeriesTypes");
        }
    }
}
