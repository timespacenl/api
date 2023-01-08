using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespace.Api.Migrations
{
    /// <inheritdoc />
    public partial class PrimaryIdentifierColumnAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Primary",
                table: "IdentityIdentifiers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Primary",
                table: "IdentityIdentifiers");
        }
    }
}
