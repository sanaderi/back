using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumContentKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurriculumBoardId",
                table: "Tutorials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurriculumLinkDate",
                table: "Tutorials",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumQualificationId",
                table: "Tutorials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumSyllabusId",
                table: "Tutorials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumBoardId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurriculumLinkDate",
                table: "PastPapers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumPaperId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumQualificationId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumSeriesTypeId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumSyllabusId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumVariantOptionId",
                table: "PastPapers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumBoardId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CurriculumLinkDate",
                table: "Exams",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumQualificationId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumSyllabusId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurriculumContentTopics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentKind = table.Column<byte>(type: "tinyint", nullable: false),
                    ContentId = table.Column<long>(type: "bigint", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumContentTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumContentTopics_CurriculumTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "CurriculumTopics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_CurriculumLinkDate",
                table: "Tutorials",
                column: "CurriculumLinkDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_CurriculumSyllabusId",
                table: "Tutorials",
                column: "CurriculumSyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_PastPapers_CurriculumLinkDate",
                table: "PastPapers",
                column: "CurriculumLinkDate");

            migrationBuilder.CreateIndex(
                name: "IX_PastPapers_CurriculumSyllabusId",
                table: "PastPapers",
                column: "CurriculumSyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CurriculumLinkDate",
                table: "Exams",
                column: "CurriculumLinkDate");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CurriculumSyllabusId",
                table: "Exams",
                column: "CurriculumSyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumContentTopics_ContentKind_ContentId_TopicId",
                table: "CurriculumContentTopics",
                columns: new[] { "ContentKind", "ContentId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumContentTopics_TopicId_ContentKind",
                table: "CurriculumContentTopics",
                columns: new[] { "TopicId", "ContentKind" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumContentTopics");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_CurriculumLinkDate",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_CurriculumSyllabusId",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_PastPapers_CurriculumLinkDate",
                table: "PastPapers");

            migrationBuilder.DropIndex(
                name: "IX_PastPapers_CurriculumSyllabusId",
                table: "PastPapers");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CurriculumLinkDate",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CurriculumSyllabusId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CurriculumBoardId",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "CurriculumLinkDate",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "CurriculumQualificationId",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "CurriculumSyllabusId",
                table: "Tutorials");

            migrationBuilder.DropColumn(
                name: "CurriculumBoardId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumLinkDate",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumPaperId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumQualificationId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumSeriesTypeId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumSyllabusId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumVariantOptionId",
                table: "PastPapers");

            migrationBuilder.DropColumn(
                name: "CurriculumBoardId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CurriculumLinkDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CurriculumQualificationId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CurriculumSyllabusId",
                table: "Exams");
        }
    }
}
