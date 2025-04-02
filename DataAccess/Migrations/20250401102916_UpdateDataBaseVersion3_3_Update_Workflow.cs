using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_3_Update_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FlowNumber",
                table: "WorkflowFlow",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "FlowNumber",
                table: "WorkflowFlow",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
