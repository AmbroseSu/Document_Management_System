using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion4_1_Doc_Signature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSignature_Document_DocumentId",
                table: "DocumentSignature");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "DocumentSignature",
                newName: "DocumentVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSignature_DocumentId",
                table: "DocumentSignature",
                newName: "IX_DocumentSignature_DocumentVersionId");

            migrationBuilder.Sql(
                "ALTER TABLE \"Document\" ALTER COLUMN \"DateReceived\" TYPE timestamp with time zone USING \"DateReceived\"::timestamp with time zone;");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSignature_DocumentVersion_DocumentVersionId",
                table: "DocumentSignature",
                column: "DocumentVersionId",
                principalTable: "DocumentVersion",
                principalColumn: "DocumentVersionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSignature_DocumentVersion_DocumentVersionId",
                table: "DocumentSignature");

            migrationBuilder.RenameColumn(
                name: "DocumentVersionId",
                table: "DocumentSignature",
                newName: "DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentSignature_DocumentVersionId",
                table: "DocumentSignature",
                newName: "IX_DocumentSignature_DocumentId");

            migrationBuilder.AlterColumn<string>(
                name: "DateReceived",
                table: "Document",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSignature_Document_DocumentId",
                table: "DocumentSignature",
                column: "DocumentId",
                principalTable: "Document",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
