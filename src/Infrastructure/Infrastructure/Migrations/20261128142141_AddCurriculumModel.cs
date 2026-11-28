using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurriculumBoards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    ParentBoardId = table.Column<int>(type: "int", nullable: true),
                    Logo = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    Website = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    IsMaster = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false),
                    CodeTemplate = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumBoards_CurriculumBoards_ParentBoardId",
                        column: x => x.ParentBoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurriculumBoards_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumDocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShortLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumDocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Picture = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumBoardFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Style = table.Column<byte>(type: "tinyint", nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumBoardFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumBoardFilters_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumBoardLinkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    OtherBoardId = table.Column<int>(type: "int", nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumBoardLinkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumBoardLinkOrders_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumBoardLinkOrders_CurriculumBoards_OtherBoardId",
                        column: x => x.OtherBoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumBoardTabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    Tab = table.Column<byte>(type: "tinyint", nullable: false),
                    Scope = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumBoardTabs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumBoardTabs_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumQualifications_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurriculumQualifications_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSeoTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: true),
                    Page = table.Column<byte>(type: "tinyint", nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSeoTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumSeoTemplates_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumVariantKinds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumVariantKinds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumVariantKinds_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSeriesTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FileCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    Months = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    IsSupplementary = table.Column<bool>(type: "bit", nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSeriesTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumSeriesTypes_CurriculumBoards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "CurriculumBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumSeriesTypes_CurriculumQualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "CurriculumQualifications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSyllabus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    YearFrom = table.Column<short>(type: "smallint", nullable: true),
                    YearTo = table.Column<short>(type: "smallint", nullable: true),
                    Picture = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSyllabus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumSyllabus_CurriculumQualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "CurriculumQualifications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurriculumSyllabus_CurriculumSubjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "CurriculumSubjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumVariantOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    VariantKindId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumVariantOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumVariantOptions_CurriculumVariantKinds_VariantKindId",
                        column: x => x.VariantKindId,
                        principalTable: "CurriculumVariantKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumPapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    SyllabusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShortLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ListOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumPapers_CurriculumSyllabus_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "CurriculumSyllabus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    SyllabusId = table.Column<int>(type: "int", nullable: false),
                    ParentTopicId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsUnit = table.Column<bool>(type: "bit", nullable: false),
                    ListOrder = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifyDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifyUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumTopics_CurriculumSyllabus_SyllabusId",
                        column: x => x.SyllabusId,
                        principalTable: "CurriculumSyllabus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurriculumTopics_CurriculumTopics_ParentTopicId",
                        column: x => x.ParentTopicId,
                        principalTable: "CurriculumTopics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumTopicLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterTopicId = table.Column<int>(type: "int", nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    LegacyRelationId = table.Column<int>(type: "int", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreationUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumTopicLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumTopicLinks_CurriculumTopics_MasterTopicId",
                        column: x => x.MasterTopicId,
                        principalTable: "CurriculumTopics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurriculumTopicLinks_CurriculumTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "CurriculumTopics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoardFilters_BoardId_Key",
                table: "CurriculumBoardFilters",
                columns: new[] { "BoardId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoardLinkOrders_BoardId_OtherBoardId",
                table: "CurriculumBoardLinkOrders",
                columns: new[] { "BoardId", "OtherBoardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoardLinkOrders_OtherBoardId",
                table: "CurriculumBoardLinkOrders",
                column: "OtherBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_IsDefault",
                table: "CurriculumBoards",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_IsMaster",
                table: "CurriculumBoards",
                column: "IsMaster",
                unique: true,
                filter: "[IsMaster] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_LanguageId",
                table: "CurriculumBoards",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_LegacyId",
                table: "CurriculumBoards",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_ParentBoardId",
                table: "CurriculumBoards",
                column: "ParentBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoards_Slug",
                table: "CurriculumBoards",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumBoardTabs_BoardId_Tab",
                table: "CurriculumBoardTabs",
                columns: new[] { "BoardId", "Tab" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumDocumentTypes_Code",
                table: "CurriculumDocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumDocumentTypes_LegacyId",
                table: "CurriculumDocumentTypes",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPapers_LegacyId",
                table: "CurriculumPapers",
                column: "LegacyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumPapers_SyllabusId_ListOrder",
                table: "CurriculumPapers",
                columns: new[] { "SyllabusId", "ListOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumQualifications_BoardId_Slug",
                table: "CurriculumQualifications",
                columns: new[] { "BoardId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumQualifications_LanguageId",
                table: "CurriculumQualifications",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumQualifications_LegacyId",
                table: "CurriculumQualifications",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSeoTemplates_BoardId_Page",
                table: "CurriculumSeoTemplates",
                columns: new[] { "BoardId", "Page" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSeriesTypes_BoardId_QualificationId_ListOrder",
                table: "CurriculumSeriesTypes",
                columns: new[] { "BoardId", "QualificationId", "ListOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSeriesTypes_QualificationId",
                table: "CurriculumSeriesTypes",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSubjects_Slug",
                table: "CurriculumSubjects",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSyllabus_Code",
                table: "CurriculumSyllabus",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSyllabus_LegacyId",
                table: "CurriculumSyllabus",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSyllabus_QualificationId_Slug",
                table: "CurriculumSyllabus",
                columns: new[] { "QualificationId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSyllabus_SubjectId",
                table: "CurriculumSyllabus",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopicLinks_LegacyRelationId",
                table: "CurriculumTopicLinks",
                column: "LegacyRelationId",
                unique: true,
                filter: "[LegacyRelationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopicLinks_MasterTopicId_TopicId",
                table: "CurriculumTopicLinks",
                columns: new[] { "MasterTopicId", "TopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopicLinks_TopicId",
                table: "CurriculumTopicLinks",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopics_LegacyId",
                table: "CurriculumTopics",
                column: "LegacyId",
                unique: true,
                filter: "[LegacyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopics_ParentTopicId",
                table: "CurriculumTopics",
                column: "ParentTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTopics_SyllabusId_ParentTopicId_ListOrder",
                table: "CurriculumTopics",
                columns: new[] { "SyllabusId", "ParentTopicId", "ListOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVariantKinds_BoardId_Key",
                table: "CurriculumVariantKinds",
                columns: new[] { "BoardId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVariantOptions_LegacyId",
                table: "CurriculumVariantOptions",
                column: "LegacyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVariantOptions_VariantKindId_ListOrder",
                table: "CurriculumVariantOptions",
                columns: new[] { "VariantKindId", "ListOrder" });

            // The default SEO templates (no board): the website's own titles and descriptions as of 2026-11-28, written as
            // templates (see CurriculumSeoDefaults), plus the new landing pages'. Literal here so the migration never
            // changes; an admin edits them afterwards.
            migrationBuilder.InsertData(
                table: "CurriculumSeoTemplates",
                columns: ["Id", "Page", "TitleTemplate", "DescriptionTemplate"],
                columnTypes: ["int", "tinyint", "nvarchar(500)", "nvarchar(1000)"],
                values: new object[,]
                {
                    { 1, (byte)0, "{board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Past Papers || Educational Resources | K12 Education Papers and Materials", "Download list of {board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Past Papers. Includes mark scheme for exam preparation. || Enhance your learning with GamaTrain's extensive collection of online documents and texts, carefully curated to enrich your academic journey." },
                    { 2, (byte)1, "{board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Study Materials || Educational Resources | K12 Education Papers and Materials", "Download list of {board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}]. Useful for study, practice, and exam preparation. || Enhance your learning with GamaTrain's extensive collection of online documents and texts, carefully curated to enrich your academic journey." },
                    { 3, (byte)2, "{board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Exams || Online Exams, Free Exams for Improving Education", "Download list of {board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Online test. Useful for study, practice, and exam preparation. || Hone your skills and assess your knowledge with GamaTrain's online exams, designed to enhance your exam preparation and boost your confidence." },
                    { 4, (byte)3, "{board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Revision notes || Master Concepts, Enhance Learning: GamaTrain's Online Tutorials", "Download list of {board}[ {qualification}][ {subject}][ {classification}][ {series}][ {year}] Textbook. Useful for study, practice, and exam preparation. || Complement your studies with GamaTrain's comprehensive online tutorials, providing step-by-step guidance and practice opportunities to refine your understanding." },
                    { 5, (byte)4, "[{board} ][{qualification} ]{title} past paper", "Download [{board} ][{qualification} ]{title} past paper with mark scheme (MS). Access a full collection of past papers for study, revision, and exam practice." },
                    { 6, (byte)7, "[{board} ][{qualification} ]{title}", "Download {title} – {qualification}, {board} curriculum. Ideal for quick revision, practice, and exam prep. || Download {title}. Ideal for quick revision, practice, and exam prep." },
                    { 7, (byte)5, "{subject}[ {kind}] – {format}", "{subject}[ {kind}][: {facts}]. {cta} PDF, Word and PowerPoint too." },
                    { 8, (byte)6, "[{board} ][{qualification} ]{title}", "Learn {title} with step-by-step explanations and examples from the {qualification} {board} curriculum. || Learn {title} with step-by-step explanations and examples." },
                    { 9, (byte)8, "{board} Past Papers, Exams and Revision Notes", "Free {board} past papers with mark schemes, online exams and revision notes[ for {qualifications}]. Find {board} schools[ in {city}]." },
                    { 10, (byte)9, "{board} {qualification} Past Papers and Revision Notes", "{board} {qualification} past papers with mark schemes, online exams and revision notes for every subject. Find schools offering {qualification}[ in {city}]." },
                    { 11, (byte)10, "{board} {qualification} {subject}[ {syllabusCode}] Past Papers", "{board} {qualification} {subject}[ {syllabusCode}] past papers with mark schemes[ {yearFrom}–{yearTo}], revision notes by topic and online exams. Find schools teaching it[ in {city}]." },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumBoardFilters");

            migrationBuilder.DropTable(
                name: "CurriculumBoardLinkOrders");

            migrationBuilder.DropTable(
                name: "CurriculumBoardTabs");

            migrationBuilder.DropTable(
                name: "CurriculumDocumentTypes");

            migrationBuilder.DropTable(
                name: "CurriculumPapers");

            migrationBuilder.DropTable(
                name: "CurriculumSeoTemplates");

            migrationBuilder.DropTable(
                name: "CurriculumSeriesTypes");

            migrationBuilder.DropTable(
                name: "CurriculumTopicLinks");

            migrationBuilder.DropTable(
                name: "CurriculumVariantOptions");

            migrationBuilder.DropTable(
                name: "CurriculumTopics");

            migrationBuilder.DropTable(
                name: "CurriculumVariantKinds");

            migrationBuilder.DropTable(
                name: "CurriculumSyllabus");

            migrationBuilder.DropTable(
                name: "CurriculumQualifications");

            migrationBuilder.DropTable(
                name: "CurriculumSubjects");

            migrationBuilder.DropTable(
                name: "CurriculumBoards");
        }
    }
}
