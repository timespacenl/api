using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Timespace.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationTableIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Verification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    IdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifableIdentityIdentifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiableIdentityIdentifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationTokenType = table.Column<string>(type: "text", nullable: false),
                    VerificationToken = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verification_Identities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "Identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Verification_IdentityIdentifiers_VerifiableIdentityIdentifi~",
                        column: x => x.VerifiableIdentityIdentifierId,
                        principalTable: "IdentityIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verification_IdentityId",
                table: "Verification",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_VerifiableIdentityIdentifierId",
                table: "Verification",
                column: "VerifiableIdentityIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_VerificationToken",
                table: "Verification",
                column: "VerificationToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Verification");
        }
    }
}
