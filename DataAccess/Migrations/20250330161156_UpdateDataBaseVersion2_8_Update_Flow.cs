using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion2_8_Update_Flow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextFlowId",
                table: "Flow");

            migrationBuilder.AddColumn<string>(
                name: "RoleEnd",
                table: "Flow",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleStart",
                table: "Flow",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleEnd",
                table: "Flow");

            migrationBuilder.DropColumn(
                name: "RoleStart",
                table: "Flow");

            migrationBuilder.AddColumn<Guid>(
                name: "NextFlowId",
                table: "Flow",
                type: "uuid",
                nullable: true);
        }
    }
}
