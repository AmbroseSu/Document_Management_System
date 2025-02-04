using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Division_DivisionId",
                table: "User");

            migrationBuilder.AlterColumn<Guid>(
                name: "DivisionId",
                table: "User",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Division_DivisionId",
                table: "User",
                column: "DivisionId",
                principalTable: "Division",
                principalColumn: "DivisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Division_DivisionId",
                table: "User");

            migrationBuilder.AlterColumn<Guid>(
                name: "DivisionId",
                table: "User",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Division_DivisionId",
                table: "User",
                column: "DivisionId",
                principalTable: "Division",
                principalColumn: "DivisionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
