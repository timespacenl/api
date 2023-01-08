using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespace.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdentityIdentifierTenantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "IdentityIdentifiers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_IdentityIdentifiers_TenantId",
                table: "IdentityIdentifiers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityIdentifiers_Tenants_TenantId",
                table: "IdentityIdentifiers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityIdentifiers_Tenants_TenantId",
                table: "IdentityIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_IdentityIdentifiers_TenantId",
                table: "IdentityIdentifiers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IdentityIdentifiers");
        }
    }
}
