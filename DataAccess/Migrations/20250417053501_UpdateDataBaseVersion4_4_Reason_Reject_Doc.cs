using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion4_4_Reason_Reject_Doc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Tasks_TaskId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Comment",
                newName: "DocumentVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_TaskId",
                table: "Comment",
                newName: "IX_Comment_DocumentVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_DocumentVersion_DocumentVersionId",
                table: "Comment",
                column: "DocumentVersionId",
                principalTable: "DocumentVersion",
                principalColumn: "DocumentVersionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_DocumentVersion_DocumentVersionId",
                table: "Comment");

            migrationBuilder.RenameColumn(
                name: "DocumentVersionId",
                table: "Comment",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_DocumentVersionId",
                table: "Comment",
                newName: "IX_Comment_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Tasks_TaskId",
                table: "Comment",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
