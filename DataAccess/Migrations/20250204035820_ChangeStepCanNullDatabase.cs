using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStepCanNullDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role");

            migrationBuilder.AlterColumn<Guid>(
                name: "StepId",
                table: "Role",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role",
                column: "StepId",
                principalTable: "Step",
                principalColumn: "StepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role");

            migrationBuilder.AlterColumn<Guid>(
                name: "StepId",
                table: "Role",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role",
                column: "StepId",
                principalTable: "Step",
                principalColumn: "StepId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
