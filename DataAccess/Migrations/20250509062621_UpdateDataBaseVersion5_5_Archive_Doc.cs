using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion5_5_Archive_Doc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument",
                column: "DocumentReplaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument",
                column: "DocumentRevokeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument",
                column: "DocumentReplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument",
                column: "DocumentRevokeId");
        }
    }
}
