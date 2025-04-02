using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_2_Update_Step : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAt",
                table: "Workflow",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                table: "Workflow",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredRolesJson",
                table: "Workflow",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RejectStepId",
                table: "Step",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAt",
                table: "DocumentType",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                table: "DocumentType",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateAt",
                table: "Division",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                table: "Division",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateAt",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "RequiredRolesJson",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "RejectStepId",
                table: "Step");

            migrationBuilder.DropColumn(
                name: "CreateAt",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "DocumentType");

            migrationBuilder.DropColumn(
                name: "CreateAt",
                table: "Division");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "Division");
        }
    }
}
