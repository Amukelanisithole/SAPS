using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStationCoordinatesAndReportRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Stations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Stations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ComplainantLatitude",
                table: "PreliminaryReports",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ComplainantLongitude",
                table: "PreliminaryReports",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NearestStationId",
                table: "PreliminaryReports",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreliminaryReports_NearestStationId",
                table: "PreliminaryReports",
                column: "NearestStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreliminaryReports_Stations_NearestStationId",
                table: "PreliminaryReports",
                column: "NearestStationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreliminaryReports_Stations_NearestStationId",
                table: "PreliminaryReports");

            migrationBuilder.DropIndex(
                name: "IX_PreliminaryReports_NearestStationId",
                table: "PreliminaryReports");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "ComplainantLatitude",
                table: "PreliminaryReports");

            migrationBuilder.DropColumn(
                name: "ComplainantLongitude",
                table: "PreliminaryReports");

            migrationBuilder.DropColumn(
                name: "NearestStationId",
                table: "PreliminaryReports");
        }
    }
}
