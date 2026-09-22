using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAffidavits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Affidavits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplainantId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    WorkingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TelWork = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TelHome = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TelCell = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeclarationText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectionToOath = table.Column<bool>(type: "bit", nullable: false),
                    VerificationPhotoPath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SubmissionTimestamp = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    ComplainantLatitude = table.Column<double>(type: "float", nullable: true),
                    ComplainantLongitude = table.Column<double>(type: "float", nullable: true),
                    NearestStationId = table.Column<long>(type: "bigint", nullable: true),
                    StampedByOfficerId = table.Column<long>(type: "bigint", nullable: true),
                    StampedByOfficerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StampedByOfficerForceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StampedByOfficerRank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StampedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    StampedPdfPath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affidavits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Affidavits_Stations_NearestStationId",
                        column: x => x.NearestStationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Affidavits_Users_ComplainantId",
                        column: x => x.ComplainantId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Affidavits_Users_StampedByOfficerId",
                        column: x => x.StampedByOfficerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Affidavits_ComplainantId",
                table: "Affidavits",
                column: "ComplainantId");

            migrationBuilder.CreateIndex(
                name: "IX_Affidavits_NearestStationId",
                table: "Affidavits",
                column: "NearestStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Affidavits_ReferenceNumber",
                table: "Affidavits",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affidavits_StampedByOfficerId",
                table: "Affidavits",
                column: "StampedByOfficerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Affidavits");
        }
    }
}
