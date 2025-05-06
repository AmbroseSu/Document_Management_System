using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion5_4_Fix_DB_Add_System_Number_Doc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrantPermission",
                table: "UserDocumentPermission",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Document",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SystemNumberOfDoc",
                table: "Document",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "ArchivedDocument",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SystemNumberOfDoc",
                table: "ArchivedDocument",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrantPermission",
                table: "UserDocumentPermission");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "SystemNumberOfDoc",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "SystemNumberOfDoc",
                table: "ArchivedDocument");
        }
    }
}
