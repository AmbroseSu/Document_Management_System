using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_Update_Doc_Status_WF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentWorkflowStatus",
                columns: table => new
                {
                    DocumentWorkflowStatusId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StatusDocWorkflow = table.Column<int>(type: "integer", nullable: false),
                    StatusDoc = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentWorkflowFlowId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflowStatus", x => x.DocumentWorkflowStatusId);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStatus_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStatus_WorkflowFlow_CurrentWorkflowFlowId",
                        column: x => x.CurrentWorkflowFlowId,
                        principalTable: "WorkflowFlow",
                        principalColumn: "WorkflowFlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStatus_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStatus_CurrentWorkflowFlowId",
                table: "DocumentWorkflowStatus",
                column: "CurrentWorkflowFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStatus_DocumentId",
                table: "DocumentWorkflowStatus",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStatus_WorkflowId",
                table: "DocumentWorkflowStatus",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentWorkflowStatus");
        }
    }
}
