using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_9_Signature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureValue",
                table: "DocumentSignature");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "DocumentSignature");

            migrationBuilder.DropColumn(
                name: "HashAlgorithm",
                table: "DigitalCertificate");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "DigitalCertificate");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "DigitalCertificate");

            migrationBuilder.DropColumn(
                name: "SignatureValue",
                table: "ArchiveDocumentSignature");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ArchiveDocumentSignature");

            migrationBuilder.RenameColumn(
                name: "PublicKey",
                table: "DigitalCertificate",
                newName: "Subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "DigitalCertificate",
                newName: "PublicKey");

            migrationBuilder.AddColumn<string>(
                name: "SignatureValue",
                table: "DocumentSignature",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "DocumentSignature",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "HashAlgorithm",
                table: "DigitalCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "DigitalCertificate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "DigitalCertificate",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureValue",
                table: "ArchiveDocumentSignature",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ArchiveDocumentSignature",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
