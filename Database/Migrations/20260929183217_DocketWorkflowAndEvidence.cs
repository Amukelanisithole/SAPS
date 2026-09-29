using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class DocketWorkflowAndEvidence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "PreliminaryReports",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReviewedByOfficerId",
                table: "PreliminaryReports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClosedByUserId",
                table: "Dockets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Dockets",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosureNotes",
                table: "Dockets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClosureOutcomeId",
                table: "Dockets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InvestigatingOfficerId",
                table: "Dockets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocketEvidence",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocketId = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocketEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocketEvidence_Dockets_DocketId",
                        column: x => x.DocketId,
                        principalTable: "Dockets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocketEvidence_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocketStatusHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocketId = table.Column<long>(type: "bigint", nullable: false),
                    FromStatusId = table.Column<long>(type: "bigint", nullable: true),
                    ToStatusId = table.Column<long>(type: "bigint", nullable: false),
                    ChangedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocketStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocketStatusHistory_Dockets_DocketId",
                        column: x => x.DocketId,
                        principalTable: "Dockets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocketStatusHistory_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreliminaryReports_ReviewedByOfficerId",
                table: "PreliminaryReports",
                column: "ReviewedByOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Dockets_ClosedByUserId",
                table: "Dockets",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dockets_InvestigatingOfficerId",
                table: "Dockets",
                column: "InvestigatingOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketEvidence_DocketId",
                table: "DocketEvidence",
                column: "DocketId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketEvidence_UploadedByUserId",
                table: "DocketEvidence",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketStatusHistory_ChangedByUserId",
                table: "DocketStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketStatusHistory_DocketId",
                table: "DocketStatusHistory",
                column: "DocketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dockets_Users_ClosedByUserId",
                table: "Dockets",
                column: "ClosedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dockets_Users_InvestigatingOfficerId",
                table: "Dockets",
                column: "InvestigatingOfficerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PreliminaryReports_Users_ReviewedByOfficerId",
                table: "PreliminaryReports",
                column: "ReviewedByOfficerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
UPDATE [Dockets] SET [InvestigatingOfficerId] = [ReviewingOfficerId] WHERE [InvestigatingOfficerId] IS NULL;

INSERT INTO [DocketStatusHistory] ([DocketId], [FromStatusId], [ToStatusId], [ChangedByUserId], [Reason], [ChangedDate])
SELECT [Id], NULL, 1, [ReviewingOfficerId], N'Docket opened from preliminary report.', [OpenedDate]
FROM [Dockets];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dockets_Users_ClosedByUserId",
                table: "Dockets");

            migrationBuilder.DropForeignKey(
                name: "FK_Dockets_Users_InvestigatingOfficerId",
                table: "Dockets");

            migrationBuilder.DropForeignKey(
                name: "FK_PreliminaryReports_Users_ReviewedByOfficerId",
                table: "PreliminaryReports");

            migrationBuilder.DropTable(
                name: "DocketEvidence");

            migrationBuilder.DropTable(
                name: "DocketStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_PreliminaryReports_ReviewedByOfficerId",
                table: "PreliminaryReports");

            migrationBuilder.DropIndex(
                name: "IX_Dockets_ClosedByUserId",
                table: "Dockets");

            migrationBuilder.DropIndex(
                name: "IX_Dockets_InvestigatingOfficerId",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PreliminaryReports");

            migrationBuilder.DropColumn(
                name: "ReviewedByOfficerId",
                table: "PreliminaryReports");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "ClosureNotes",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "ClosureOutcomeId",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "InvestigatingOfficerId",
                table: "Dockets");
        }
    }
}
