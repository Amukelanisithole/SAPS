using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class AddAffidavitIdToAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AffidavitId",
                table: "AuditLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AffidavitId",
                table: "AuditLogs",
                column: "AffidavitId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Affidavits_AffidavitId",
                table: "AuditLogs",
                column: "AffidavitId",
                principalTable: "Affidavits",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Affidavits_AffidavitId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_AffidavitId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "AffidavitId",
                table: "AuditLogs");
        }
    }
}
