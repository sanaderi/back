using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GamaEdtech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "CoreId",
                table: "ApplicationUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ApplicationUsers_UserId",
                table: "Transactions");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Transactions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_Topics_LastModifyUserId",
                table: "Topics");
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_ApplicationUsers_LastModifyUserId",
                table: "Topics");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Topics",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Topics_LastModifyUserId",
                table: "Topics",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Topics_CreationUserId",
                table: "Topics");
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_ApplicationUsers_CreationUserId",
                table: "Topics");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Topics",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Topics_CreationUserId",
                table: "Topics",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_ApplicationUsers_UserId",
                table: "Tickets");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Tickets",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_TicketReplies_CreationUserId",
                table: "TicketReplies");
            migrationBuilder.DropForeignKey(
                name: "FK_TicketReplies_ApplicationUsers_CreationUserId",
                table: "TicketReplies");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "TicketReplies",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_TicketReplies_CreationUserId",
                table: "TicketReplies",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_TestSubmissions_UserId_TestId",
                table: "TestSubmissions");
            migrationBuilder.DropForeignKey(
                name: "FK_TestSubmissions_ApplicationUsers_UserId",
                table: "TestSubmissions");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "TestSubmissions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_TestSubmissions_UserId_TestId",
                table: "TestSubmissions",
                columns: new[] { "UserId", "TestId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_Tags_LastModifyUserId",
                table: "Tags");
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_ApplicationUsers_LastModifyUserId",
                table: "Tags");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Tags",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Tags_LastModifyUserId",
                table: "Tags",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CreationUserId",
                table: "Tags");
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_ApplicationUsers_CreationUserId",
                table: "Tags");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Tags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreationUserId",
                table: "Tags",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_LastModifyUserId",
                table: "Subjects");
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_ApplicationUsers_LastModifyUserId",
                table: "Subjects");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Subjects",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Subjects_LastModifyUserId",
                table: "Subjects",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CreationUserId",
                table: "Subjects");
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_ApplicationUsers_CreationUserId",
                table: "Subjects");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Subjects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CreationUserId",
                table: "Subjects",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_SchoolTags_CreationUserId",
                table: "SchoolTags");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolTags_ApplicationUsers_CreationUserId",
                table: "SchoolTags");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "SchoolTags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_SchoolTags_CreationUserId",
                table: "SchoolTags",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Schools_LastModifyUserId",
                table: "Schools");
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_ApplicationUsers_LastModifyUserId",
                table: "Schools");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Schools",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Schools_LastModifyUserId",
                table: "Schools",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Schools_CreationUserId",
                table: "Schools");
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_ApplicationUsers_CreationUserId",
                table: "Schools");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Schools",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Schools_CreationUserId",
                table: "Schools",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_SchoolImages_LastModifyUserId",
                table: "SchoolImages");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolImages_ApplicationUsers_LastModifyUserId",
                table: "SchoolImages");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "SchoolImages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_SchoolImages_LastModifyUserId",
                table: "SchoolImages",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_SchoolImages_CreationUserId",
                table: "SchoolImages");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolImages_ApplicationUsers_CreationUserId",
                table: "SchoolImages");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "SchoolImages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_SchoolImages_CreationUserId",
                table: "SchoolImages",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_SchoolComments_LastModifyUserId",
                table: "SchoolComments");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolComments_ApplicationUsers_LastModifyUserId",
                table: "SchoolComments");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "SchoolComments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_SchoolComments_LastModifyUserId",
                table: "SchoolComments",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_SchoolComments_CreationUserId_SchoolId",
                table: "SchoolComments");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolComments_ApplicationUsers_CreationUserId",
                table: "SchoolComments");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "SchoolComments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_SchoolComments_CreationUserId_SchoolId",
                table: "SchoolComments",
                columns: new[] { "CreationUserId", "SchoolId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_SchoolBoards_CreationUserId",
                table: "SchoolBoards");
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolBoards_ApplicationUsers_CreationUserId",
                table: "SchoolBoards");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "SchoolBoards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_SchoolBoards_CreationUserId",
                table: "SchoolBoards",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_CategoryType_IdentifierId_CreationUserId",
                table: "Reactions");
            migrationBuilder.DropIndex(
                name: "IX_Reactions_CreationUserId",
                table: "Reactions");
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_ApplicationUsers_CreationUserId",
                table: "Reactions");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Reactions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Reactions_CreationUserId",
                table: "Reactions",
                column: "CreationUserId");
            migrationBuilder.CreateIndex(
                name: "IX_Reactions_CategoryType_IdentifierId_CreationUserId",
                table: "Reactions",
                columns: new[] { "CategoryType", "IdentifierId", "CreationUserId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_Questions_LastModifyUserId",
                table: "Questions");
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ApplicationUsers_LastModifyUserId",
                table: "Questions");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Questions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Questions_LastModifyUserId",
                table: "Questions",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreationUserId",
                table: "Questions");
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_ApplicationUsers_CreationUserId",
                table: "Questions");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Questions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreationUserId",
                table: "Questions",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_PostTags_CreationUserId",
                table: "PostTags");
            migrationBuilder.DropForeignKey(
                name: "FK_PostTags_ApplicationUsers_CreationUserId",
                table: "PostTags");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "PostTags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_PostTags_CreationUserId",
                table: "PostTags",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Posts_LastModifyUserId",
                table: "Posts");
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_ApplicationUsers_LastModifyUserId",
                table: "Posts");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Posts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Posts_LastModifyUserId",
                table: "Posts",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreationUserId",
                table: "Posts");
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_ApplicationUsers_CreationUserId",
                table: "Posts");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Posts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreationUserId",
                table: "Posts",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_LastModifyUserId",
                table: "PostComments");
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_ApplicationUsers_LastModifyUserId",
                table: "PostComments");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "PostComments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_PostComments_LastModifyUserId",
                table: "PostComments",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_CreationUserId_PostId",
                table: "PostComments");
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_ApplicationUsers_CreationUserId",
                table: "PostComments");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "PostComments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_PostComments_CreationUserId_PostId",
                table: "PostComments",
                columns: new[] { "CreationUserId", "PostId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId",
                table: "Payments");
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ApplicationUsers_UserId",
                table: "Payments");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Payments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ApplicationUsers_SenderId",
                table: "Messages");
            migrationBuilder.AlterColumn<long>(
                name: "SenderId",
                table: "Messages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages");
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ApplicationUsers_ReceiverId",
                table: "Messages");
            migrationBuilder.AlterColumn<long>(
                name: "ReceiverId",
                table: "Messages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.DropIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories");
            migrationBuilder.DropForeignKey(
                name: "FK_LoginHistories_ApplicationUsers_UserId",
                table: "LoginHistories");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "LoginHistories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_Locations_LastModifyUserId",
                table: "Locations");
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_ApplicationUsers_LastModifyUserId",
                table: "Locations");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Locations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Locations_LastModifyUserId",
                table: "Locations",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CreationUserId",
                table: "Locations");
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_ApplicationUsers_CreationUserId",
                table: "Locations");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Locations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Locations_CreationUserId",
                table: "Locations",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Grades_LastModifyUserId",
                table: "Grades");
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_ApplicationUsers_LastModifyUserId",
                table: "Grades");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Grades",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Grades_LastModifyUserId",
                table: "Grades",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Grades_CreationUserId",
                table: "Grades");
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_ApplicationUsers_CreationUserId",
                table: "Grades");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Grades",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Grades_CreationUserId",
                table: "Grades",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_UserId",
                table: "Experiences");
            migrationBuilder.DropForeignKey(
                name: "FK_Experiences_ApplicationUsers_UserId",
                table: "Experiences");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Experiences",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Experiences_UserId",
                table: "Experiences",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubmissions_UserId_ExamId",
                table: "ExamSubmissions");
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubmissions_ApplicationUsers_UserId",
                table: "ExamSubmissions");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ExamSubmissions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_UserId_ExamId",
                table: "ExamSubmissions",
                columns: new[] { "UserId", "ExamId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_Contributions_LastModifyUserId",
                table: "Contributions");
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_ApplicationUsers_LastModifyUserId",
                table: "Contributions");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Contributions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Contributions_LastModifyUserId",
                table: "Contributions",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Contributions_CreationUserId",
                table: "Contributions");
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_ApplicationUsers_CreationUserId",
                table: "Contributions");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Contributions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Contributions_CreationUserId",
                table: "Contributions",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_ContentLocalizations_LastModifyUserId",
                table: "ContentLocalizations");
            migrationBuilder.DropForeignKey(
                name: "FK_ContentLocalizations_ApplicationUsers_LastModifyUserId",
                table: "ContentLocalizations");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "ContentLocalizations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_ContentLocalizations_LastModifyUserId",
                table: "ContentLocalizations",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_ContentLocalizations_CreationUserId",
                table: "ContentLocalizations");
            migrationBuilder.DropForeignKey(
                name: "FK_ContentLocalizations_ApplicationUsers_CreationUserId",
                table: "ContentLocalizations");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "ContentLocalizations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ContentLocalizations_CreationUserId",
                table: "ContentLocalizations",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections");
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_ApplicationUsers_SourceUserId",
                table: "Connections");
            migrationBuilder.AlterColumn<long>(
                name: "SourceUserId",
                table: "Connections",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropForeignKey(
                name: "FK_Connections_ApplicationUsers_DestinationUserId",
                table: "Connections");
            migrationBuilder.AlterColumn<long>(
                name: "DestinationUserId",
                table: "Connections",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Connections_SourceUserId_DestinationUserId",
                table: "Connections",
                columns: new[] { "SourceUserId", "DestinationUserId" });

            migrationBuilder.DropIndex(
                name: "IX_Boards_LastModifyUserId",
                table: "Boards");
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_ApplicationUsers_LastModifyUserId",
                table: "Boards");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "Boards",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_Boards_LastModifyUserId",
                table: "Boards",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_Boards_CreationUserId",
                table: "Boards");
            migrationBuilder.DropForeignKey(
                name: "FK_Boards_ApplicationUsers_CreationUserId",
                table: "Boards");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "Boards",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_Boards_CreationUserId",
                table: "Boards",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserLogin_UserId",
                table: "ApplicationUserLogins");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserLogins_ApplicationUsers_UserId",
                table: "ApplicationUserLogins");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ApplicationUserLogins",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserLogin_UserId",
                table: "ApplicationUserLogins",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserClaim_UserId",
                table: "ApplicationUserClaims");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserClaims_ApplicationUsers_UserId",
                table: "ApplicationUserClaims");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ApplicationUserClaims",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserClaim_UserId",
                table: "ApplicationUserClaims",
                column: "UserId");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationSettings_LastModifyUserId",
                table: "ApplicationSettings");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationSettings_ApplicationUsers_LastModifyUserId",
                table: "ApplicationSettings");
            migrationBuilder.AlterColumn<long>(
                name: "LastModifyUserId",
                table: "ApplicationSettings",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSettings_LastModifyUserId",
                table: "ApplicationSettings",
                column: "LastModifyUserId");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationSettings_CreationUserId",
                table: "ApplicationSettings");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationSettings_ApplicationUsers_CreationUserId",
                table: "ApplicationSettings");
            migrationBuilder.AlterColumn<long>(
                name: "CreationUserId",
                table: "ApplicationSettings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSettings_CreationUserId",
                table: "ApplicationSettings",
                column: "CreationUserId");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRoleClaims_RoleId",
                table: "ApplicationRoleClaims");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRoleClaims_ApplicationRoles_RoleId",
                table: "ApplicationRoleClaims");
            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "ApplicationRoleClaims",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleClaims_RoleId",
                table: "ApplicationRoleClaims",
                column: "RoleId");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserTokens_ApplicationUsers_UserId",
                table: "ApplicationUserTokens");
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUserTokens",
                table: "ApplicationUserTokens");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ApplicationUserTokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUserTokens",
                table: "ApplicationUserTokens",
                columns: ["UserId", "LoginProvider", "Name"]);

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserRole_RoleId",
                table: "ApplicationUserRoles");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationRoles_RoleId",
                table: "ApplicationUserRoles");
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationUsers_UserId",
                table: "ApplicationUserRoles");
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUserRoles",
                table: "ApplicationUserRoles");
            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "ApplicationUserRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "ApplicationUserRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUserRoles",
                table: "ApplicationUserRoles",
                columns: ["UserId", "RoleId"]);
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserRole_RoleId",
                table: "ApplicationUserRoles",
                column: "RoleId");

            migrationBuilder.CreateTable(
                name: "ApplicationUsers0",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    SecurityStamp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    RegistrationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    SchoolId = table.Column<long>(type: "bigint", nullable: true),
                    ReferralId = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    CurrentBalance = table.Column<long>(type: "bigint", nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: true),
                    Board = table.Column<int>(type: "int", nullable: true),
                    ProfileUpdated = table.Column<bool>(type: "bit", nullable: false, defaultValue: (byte)0),
                    Group = table.Column<int>(type: "int", nullable: true),
                    CoreId = table.Column<int>(type: "int", nullable: true),
                    WalletId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ProfileVisibility = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    ProfileView = table.Column<long>(type: "bigint", nullable: false, defaultValue: (long)0),
                    Biography = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CurrentStatusSentence = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    OrphanDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Handle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastLoginDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers0", x => x.Id);
                });
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [ApplicationUsers0] ON;
        
                INSERT INTO [ApplicationUsers0] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp]
                    ,[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount],[RegistrationDate]
                    ,[Enabled],[Avatar],[FirstName],[LastName],[CityId],[SchoolId],[ReferralId],[CurrentBalance],[Gender],[Grade],[Board],[ProfileUpdated]
                    ,[Group],[CoreId],[WalletId],[ProfileVisibility],[ProfileView],[Biography],[Skills],[CurrentStatusSentence],[OrphanDate],[Handle],[LastLoginDate])
                SELECT CAST([Id] AS bigint),[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp]
                    ,[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount],[RegistrationDate]
                    ,[Enabled],[Avatar],[FirstName],[LastName],[CityId],[SchoolId],[ReferralId],[CurrentBalance],[Gender],[Grade],[Board],[ProfileUpdated]
                    ,[Group],[CoreId],[WalletId],[ProfileVisibility],[ProfileView],[Biography],[Skills],[CurrentStatusSentence],[OrphanDate],[Handle],[LastLoginDate]
                FROM [ApplicationUsers];

                SET IDENTITY_INSERT [ApplicationUsers0] OFF;
            ");
            migrationBuilder.DropTable(name: "ApplicationUsers");
            migrationBuilder.RenameTable(name: "ApplicationUsers0", newName: "ApplicationUsers");
            migrationBuilder.Sql(@"EXEC sp_rename 'PK_ApplicationUsers0', 'PK_ApplicationUsers', 'OBJECT';");
            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Locations_CityId",
                table: "ApplicationUsers",
                column: "CityId",
                principalTable: "Locations",
                principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Schools_SchoolId",
                table: "ApplicationUsers",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_Handle",
                table: "ApplicationUsers",
                column: "Handle",
                unique: true,
                filter: "([Handle] IS NOT NULL)");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NormalizedEmail",
                table: "ApplicationUsers",
                column: "NormalizedEmail");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_NormalizedUserName",
                table: "ApplicationUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "([NormalizedUserName] IS NOT NULL)");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_ReferralId",
                table: "ApplicationUsers",
                column: "ReferralId",
                unique: true,
                filter: "([ReferralId] IS NOT NULL)");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_CityId",
                table: "ApplicationUsers",
                column: "CityId");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_SchoolId",
                table: "ApplicationUsers",
                column: "SchoolId");

            migrationBuilder.CreateTable(
                name: "ApplicationRoles0",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoles0", x => x.Id);
                });
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [ApplicationRoles0] ON;
        
                INSERT INTO [ApplicationRoles0] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
                SELECT CAST([Id] AS bigint),[Name],[NormalizedName],[ConcurrencyStamp]
                FROM [ApplicationRoles];

                SET IDENTITY_INSERT [ApplicationRoles0] OFF;
            ");
            migrationBuilder.DropTable(name: "ApplicationRoles");
            migrationBuilder.RenameTable(name: "ApplicationRoles0", newName: "ApplicationRoles");
            migrationBuilder.Sql(@"EXEC sp_rename 'PK_ApplicationRoles0', 'PK_ApplicationRoles', 'OBJECT';");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRole_NormalizedName",
                table: "ApplicationRoles",
                column: "NormalizedName",
                unique: true,
                filter: "([NormalizedName] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ApplicationUsers_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_ApplicationUsers_LastModifyUserId",
                table: "Topics",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_ApplicationUsers_CreationUserId",
                table: "Topics",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_ApplicationUsers_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketReplies_ApplicationUsers_CreationUserId",
                table: "TicketReplies",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSubmissions_ApplicationUsers_UserId",
                table: "TestSubmissions",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_ApplicationUsers_LastModifyUserId",
                table: "Tags",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_ApplicationUsers_CreationUserId",
                table: "Tags",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_ApplicationUsers_LastModifyUserId",
                table: "Subjects",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_ApplicationUsers_CreationUserId",
                table: "Subjects",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolTags_ApplicationUsers_CreationUserId",
                table: "SchoolTags",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_ApplicationUsers_LastModifyUserId",
                table: "Schools",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_ApplicationUsers_CreationUserId",
                table: "Schools",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolImages_ApplicationUsers_LastModifyUserId",
                table: "SchoolImages",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolImages_ApplicationUsers_CreationUserId",
                table: "SchoolImages",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolComments_ApplicationUsers_LastModifyUserId",
                table: "SchoolComments",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolComments_ApplicationUsers_CreationUserId",
                table: "SchoolComments",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolBoards_ApplicationUsers_CreationUserId",
                table: "SchoolBoards",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_ApplicationUsers_CreationUserId",
                table: "Reactions",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ApplicationUsers_LastModifyUserId",
                table: "Questions",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_ApplicationUsers_CreationUserId",
                table: "Questions",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTags_ApplicationUsers_CreationUserId",
                table: "PostTags",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_ApplicationUsers_LastModifyUserId",
                table: "Posts",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_ApplicationUsers_CreationUserId",
                table: "Posts",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_ApplicationUsers_LastModifyUserId",
                table: "PostComments",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_ApplicationUsers_CreationUserId",
                table: "PostComments",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ApplicationUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ApplicationUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ApplicationUsers_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginHistories_ApplicationUsers_UserId",
                table: "LoginHistories",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_ApplicationUsers_LastModifyUserId",
                table: "Locations",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_ApplicationUsers_CreationUserId",
                table: "Locations",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_ApplicationUsers_LastModifyUserId",
                table: "Grades",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_ApplicationUsers_CreationUserId",
                table: "Grades",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiences_ApplicationUsers_UserId",
                table: "Experiences",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubmissions_ApplicationUsers_UserId",
                table: "ExamSubmissions",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_ApplicationUsers_LastModifyUserId",
                table: "Contributions",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_ApplicationUsers_CreationUserId",
                table: "Contributions",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentLocalizations_ApplicationUsers_LastModifyUserId",
                table: "ContentLocalizations",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentLocalizations_ApplicationUsers_CreationUserId",
                table: "ContentLocalizations",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_ApplicationUsers_SourceUserId",
                table: "Connections",
                column: "SourceUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_ApplicationUsers_DestinationUserId",
                table: "Connections",
                column: "DestinationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_ApplicationUsers_LastModifyUserId",
                table: "Boards",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_ApplicationUsers_CreationUserId",
                table: "Boards",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserTokens_ApplicationUsers_UserId",
                table: "ApplicationUserTokens",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationRoles_RoleId",
                table: "ApplicationUserRoles",
                column: "RoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationUsers_UserId",
                table: "ApplicationUserRoles",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserLogins_ApplicationUsers_UserId",
                table: "ApplicationUserLogins",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserClaims_ApplicationUsers_UserId",
                table: "ApplicationUserClaims",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationSettings_ApplicationUsers_LastModifyUserId",
                table: "ApplicationSettings",
                column: "LastModifyUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationSettings_ApplicationUsers_CreationUserId",
                table: "ApplicationSettings",
                column: "CreationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoleClaims_ApplicationRoles_RoleId",
                table: "ApplicationRoleClaims",
                column: "RoleId",
                principalTable: "ApplicationRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //empty
        }
    }
}
