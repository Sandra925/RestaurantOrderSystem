using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantOrderSystem.Migrations
{
    /// <inheritdoc />
    public partial class authToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthenticationMethod",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OAuthProvider",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationMethod",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OAuthProvider",
                table: "Users");
        }
    }
}
