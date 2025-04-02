using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion2_9_Update_Workflow_Flow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowTransition");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WorkflowFlow",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WorkflowFlowTransition",
                columns: table => new
                {
                    WorkflowFlowTransitionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CurrentWorkFlowFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextWorkFlowFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFlowTransition", x => x.WorkflowFlowTransitionId);
                    table.ForeignKey(
                        name: "FK_WorkflowFlowTransition_WorkflowFlow_CurrentWorkFlowFlowId",
                        column: x => x.CurrentWorkFlowFlowId,
                        principalTable: "WorkflowFlow",
                        principalColumn: "WorkflowFlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowFlowTransition_WorkflowFlow_NextWorkFlowFlowId",
                        column: x => x.NextWorkFlowFlowId,
                        principalTable: "WorkflowFlow",
                        principalColumn: "WorkflowFlowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFlowTransition_CurrentWorkFlowFlowId",
                table: "WorkflowFlowTransition",
                column: "CurrentWorkFlowFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFlowTransition_NextWorkFlowFlowId",
                table: "WorkflowFlowTransition",
                column: "NextWorkFlowFlowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowFlowTransition");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkflowFlow");

            migrationBuilder.CreateTable(
                name: "FlowTransition",
                columns: table => new
                {
                    FlowTransitionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CurrentFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextFlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowTransition", x => x.FlowTransitionId);
                    table.ForeignKey(
                        name: "FK_FlowTransition_Flow_CurrentFlowId",
                        column: x => x.CurrentFlowId,
                        principalTable: "Flow",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowTransition_Flow_NextFlowId",
                        column: x => x.NextFlowId,
                        principalTable: "Flow",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowTransition_CurrentFlowId",
                table: "FlowTransition",
                column: "CurrentFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowTransition_NextFlowId",
                table: "FlowTransition",
                column: "NextFlowId");
        }
    }
}
