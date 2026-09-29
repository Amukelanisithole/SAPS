using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class AddStationOsmNodeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OsmNodeId",
                table: "Stations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stations_OsmNodeId",
                table: "Stations",
                column: "OsmNodeId",
                unique: true,
                filter: "[OsmNodeId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stations_OsmNodeId",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "OsmNodeId",
                table: "Stations");
        }
    }
}
