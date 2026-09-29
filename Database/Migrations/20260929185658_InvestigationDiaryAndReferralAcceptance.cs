using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class InvestigationDiaryAndReferralAcceptance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Referrals_ToStationId",
                table: "Referrals");

            migrationBuilder.AddColumn<long>(
                name: "PreviousDocketStatusId",
                table: "Referrals",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RespondedByUserId",
                table: "Referrals",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedDate",
                table: "Referrals",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseNote",
                table: "Referrals",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StatusId",
                table: "Referrals",
                type: "bigint",
                nullable: false,
                defaultValue: 2L);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOverdueAlertDate",
                table: "Dockets",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "DocketEvidence",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReviewStatusId",
                table: "DocketEvidence",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "ReviewedByUserId",
                table: "DocketEvidence",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "DocketEvidence",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SubmittedByComplainant",
                table: "DocketEvidence",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DocketDiaryEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocketId = table.Column<long>(type: "bigint", nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    EntryTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SharedWithComplainant = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocketDiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocketDiaryEntries_Dockets_DocketId",
                        column: x => x.DocketId,
                        principalTable: "Dockets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocketDiaryEntries_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_RespondedByUserId",
                table: "Referrals",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ToStationId_StatusId",
                table: "Referrals",
                columns: new[] { "ToStationId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocketEvidence_ReviewedByUserId",
                table: "DocketEvidence",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketDiaryEntries_AuthorId",
                table: "DocketDiaryEntries",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocketDiaryEntries_DocketId",
                table: "DocketDiaryEntries",
                column: "DocketId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocketEvidence_Users_ReviewedByUserId",
                table: "DocketEvidence",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Users_RespondedByUserId",
                table: "Referrals",
                column: "RespondedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
INSERT INTO [DocketStatusHistory] ([DocketId], [FromStatusId], [ToStatusId], [ChangedByUserId], [Reason], [ChangedDate])
SELECT [Id], 3, 1, [ReviewingOfficerId], N'Referral received by this station.', SYSUTCDATETIME()
FROM [Dockets] WHERE [StatusId] = 3;

UPDATE [Dockets] SET [StatusId] = 1 WHERE [StatusId] = 3;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocketEvidence_Users_ReviewedByUserId",
                table: "DocketEvidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Users_RespondedByUserId",
                table: "Referrals");

            migrationBuilder.DropTable(
                name: "DocketDiaryEntries");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_RespondedByUserId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_ToStationId_StatusId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_DocketEvidence_ReviewedByUserId",
                table: "DocketEvidence");

            migrationBuilder.DropColumn(
                name: "PreviousDocketStatusId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "RespondedByUserId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "RespondedDate",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ResponseNote",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "LastOverdueAlertDate",
                table: "Dockets");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "DocketEvidence");

            migrationBuilder.DropColumn(
                name: "ReviewStatusId",
                table: "DocketEvidence");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "DocketEvidence");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "DocketEvidence");

            migrationBuilder.DropColumn(
                name: "SubmittedByComplainant",
                table: "DocketEvidence");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ToStationId",
                table: "Referrals",
                column: "ToStationId");
        }
    }
}
