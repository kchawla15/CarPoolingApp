using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarPoolingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRideModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RideId",
                table: "Rides",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RideId",
                table: "Rides");
        }
    }
}
