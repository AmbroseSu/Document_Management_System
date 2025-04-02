using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_1_Update_Relation_Doc_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentTypeWorkflow_DocumentTypeWorkflowId",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeWorkflowId",
                table: "Document",
                newName: "DocumentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_DocumentTypeWorkflowId",
                table: "Document",
                newName: "IX_Document_DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId",
                principalTable: "DocumentType",
                principalColumn: "DocumentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                table: "Document",
                newName: "DocumentTypeWorkflowId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document",
                newName: "IX_Document_DocumentTypeWorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentTypeWorkflow_DocumentTypeWorkflowId",
                table: "Document",
                column: "DocumentTypeWorkflowId",
                principalTable: "DocumentTypeWorkflow",
                principalColumn: "DocumentTypeWorkflowId");
        }
    }
}
