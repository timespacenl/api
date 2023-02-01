using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timespace.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityRelationShipFromVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verification_Identities_IdentityId",
                table: "Verification");

            migrationBuilder.DropForeignKey(
                name: "FK_Verification_IdentityIdentifiers_VerifiableIdentityIdentifi~",
                table: "Verification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Verification",
                table: "Verification");

            migrationBuilder.DropIndex(
                name: "IX_Verification_IdentityId",
                table: "Verification");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "Verification");

            migrationBuilder.RenameTable(
                name: "Verification",
                newName: "Verifications");

            migrationBuilder.RenameIndex(
                name: "IX_Verification_VerificationToken",
                table: "Verifications",
                newName: "IX_Verifications_VerificationToken");

            migrationBuilder.RenameIndex(
                name: "IX_Verification_VerifiableIdentityIdentifierId",
                table: "Verifications",
                newName: "IX_Verifications_VerifiableIdentityIdentifierId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Verifications",
                table: "Verifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Verifications_IdentityIdentifiers_VerifiableIdentityIdentif~",
                table: "Verifications",
                column: "VerifiableIdentityIdentifierId",
                principalTable: "IdentityIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verifications_IdentityIdentifiers_VerifiableIdentityIdentif~",
                table: "Verifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Verifications",
                table: "Verifications");

            migrationBuilder.RenameTable(
                name: "Verifications",
                newName: "Verification");

            migrationBuilder.RenameIndex(
                name: "IX_Verifications_VerificationToken",
                table: "Verification",
                newName: "IX_Verification_VerificationToken");

            migrationBuilder.RenameIndex(
                name: "IX_Verifications_VerifiableIdentityIdentifierId",
                table: "Verification",
                newName: "IX_Verification_VerifiableIdentityIdentifierId");

            migrationBuilder.AddColumn<Guid>(
                name: "IdentityId",
                table: "Verification",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Verification",
                table: "Verification",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_IdentityId",
                table: "Verification",
                column: "IdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Verification_Identities_IdentityId",
                table: "Verification",
                column: "IdentityId",
                principalTable: "Identities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Verification_IdentityIdentifiers_VerifiableIdentityIdentifi~",
                table: "Verification",
                column: "VerifiableIdentityIdentifierId",
                principalTable: "IdentityIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
