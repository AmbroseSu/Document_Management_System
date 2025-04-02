using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion2_7_Update_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flow_Workflow_WorkflowId",
                table: "Flow");

            migrationBuilder.DropIndex(
                name: "IX_Flow_WorkflowId",
                table: "Flow");

            migrationBuilder.DropColumn(
                name: "FlowNumber",
                table: "Flow");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "Flow");

            migrationBuilder.AddColumn<Guid>(
                name: "NextStepId",
                table: "Step",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NextFlowId",
                table: "Flow",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "WorkflowFlow",
                columns: table => new
                {
                    WorkflowFlowId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlowNumber = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFlow", x => x.WorkflowFlowId);
                    table.ForeignKey(
                        name: "FK_WorkflowFlow_Flow_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flow",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowFlow_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "WorkflowId",
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

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFlow_FlowId",
                table: "WorkflowFlow",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFlow_WorkflowId",
                table: "WorkflowFlow",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowTransition");

            migrationBuilder.DropTable(
                name: "WorkflowFlow");

            migrationBuilder.DropColumn(
                name: "NextStepId",
                table: "Step");

            migrationBuilder.DropColumn(
                name: "NextFlowId",
                table: "Flow");

            migrationBuilder.AddColumn<int>(
                name: "FlowNumber",
                table: "Flow",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowId",
                table: "Flow",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Flow_WorkflowId",
                table: "Flow",
                column: "WorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flow_Workflow_WorkflowId",
                table: "Flow",
                column: "WorkflowId",
                principalTable: "Workflow",
                principalColumn: "WorkflowId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
