using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddGymFieldsToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GymName",
                table: "Roles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "GymName",
                table: "Roles");
        }
    }
}
