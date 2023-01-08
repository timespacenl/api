using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespace.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureIdentityRelatedEntitiesAsTenantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Sessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "MfaSetupFlows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LoginFlows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "IdentityLookupSecret",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "IdentityCredentials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TenantId",
                table: "Sessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MfaSetupFlows_TenantId",
                table: "MfaSetupFlows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginFlows_TenantId",
                table: "LoginFlows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityLookupSecret_TenantId",
                table: "IdentityLookupSecret",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityCredentials_TenantId",
                table: "IdentityCredentials",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityCredentials_Tenants_TenantId",
                table: "IdentityCredentials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityLookupSecret_Tenants_TenantId",
                table: "IdentityLookupSecret",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoginFlows_Tenants_TenantId",
                table: "LoginFlows",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MfaSetupFlows_Tenants_TenantId",
                table: "MfaSetupFlows",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Tenants_TenantId",
                table: "Sessions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityCredentials_Tenants_TenantId",
                table: "IdentityCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityLookupSecret_Tenants_TenantId",
                table: "IdentityLookupSecret");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginFlows_Tenants_TenantId",
                table: "LoginFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_MfaSetupFlows_Tenants_TenantId",
                table: "MfaSetupFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Tenants_TenantId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_TenantId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_MfaSetupFlows_TenantId",
                table: "MfaSetupFlows");

            migrationBuilder.DropIndex(
                name: "IX_LoginFlows_TenantId",
                table: "LoginFlows");

            migrationBuilder.DropIndex(
                name: "IX_IdentityLookupSecret_TenantId",
                table: "IdentityLookupSecret");

            migrationBuilder.DropIndex(
                name: "IX_IdentityCredentials_TenantId",
                table: "IdentityCredentials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MfaSetupFlows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LoginFlows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IdentityLookupSecret");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IdentityCredentials");
        }
    }
}
