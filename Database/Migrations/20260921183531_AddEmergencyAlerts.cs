using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class AddEmergencyAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EmergencyAlertId",
                table: "AuditLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmergencyAlerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplainantId = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    NearestStationId = table.Column<long>(type: "bigint", nullable: true),
                    SubmissionTimestamp = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    AcknowledgedByOfficerId = table.Column<long>(type: "bigint", nullable: true),
                    AcknowledgedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Stations_NearestStationId",
                        column: x => x.NearestStationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Users_AcknowledgedByOfficerId",
                        column: x => x.AcknowledgedByOfficerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Users_ComplainantId",
                        column: x => x.ComplainantId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmergencyAlertId",
                table: "AuditLogs",
                column: "EmergencyAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_AcknowledgedByOfficerId",
                table: "EmergencyAlerts",
                column: "AcknowledgedByOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_ComplainantId",
                table: "EmergencyAlerts",
                column: "ComplainantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_NearestStationId",
                table: "EmergencyAlerts",
                column: "NearestStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_EmergencyAlerts_EmergencyAlertId",
                table: "AuditLogs",
                column: "EmergencyAlertId",
                principalTable: "EmergencyAlerts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_EmergencyAlerts_EmergencyAlertId",
                table: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EmergencyAlerts");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EmergencyAlertId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EmergencyAlertId",
                table: "AuditLogs");
        }
    }
}
