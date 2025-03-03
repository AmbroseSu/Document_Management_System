using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseWhenChangeFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentDocument_DocumentFileExtension_DocumentFileExtens~",
                table: "AttachmentDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_Deadline_User_UserId",
                table: "Deadline");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentFileExtension_DocumentFileExtensionId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Step_Workflow_WorkflowId",
                table: "Step");

            migrationBuilder.DropForeignKey(
                name: "FK_Workflow_DocumentType_DocumentTypeId",
                table: "Workflow");

            migrationBuilder.DropTable(
                name: "DocumentFileExtension");

            migrationBuilder.DropIndex(
                name: "IX_Workflow_DocumentTypeId",
                table: "Workflow");

            migrationBuilder.DropIndex(
                name: "IX_Document_DocumentFileExtensionId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_Deadline_UserId",
                table: "Deadline");

            migrationBuilder.DropIndex(
                name: "IX_AttachmentDocument_DocumentFileExtensionId",
                table: "AttachmentDocument");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "DocumentFileExtensionId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Deadline");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Deadline");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Deadline");

            migrationBuilder.DropColumn(
                name: "DocumentFileExtensionId",
                table: "AttachmentDocument");

            migrationBuilder.RenameColumn(
                name: "WorkflowId",
                table: "Step",
                newName: "FlowId");

            migrationBuilder.RenameIndex(
                name: "IX_Step_WorkflowId",
                table: "Step",
                newName: "IX_Step_FlowId");

            migrationBuilder.RenameColumn(
                name: "DocumentStatus",
                table: "Document",
                newName: "ProcessingStatus");

            migrationBuilder.RenameColumn(
                name: "DocumentNumber",
                table: "Document",
                newName: "SignedBy");

            migrationBuilder.RenameColumn(
                name: "DocumentCode",
                table: "Document",
                newName: "Sender");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllocate",
                table: "Workflow",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Workflow",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureImage",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "TaskUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "TaskUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskUserStatus",
                table: "TaskUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DateIssued",
                table: "Document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateReceived",
                table: "Document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentContent",
                table: "Document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberOfDocument",
                table: "Document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedDocumentContent",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedDocumentStatus",
                table: "ArchivedDocument",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateIssued",
                table: "ArchivedDocument",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReceived",
                table: "ArchivedDocument",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSented",
                table: "ArchivedDocument",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentReplaceId",
                table: "ArchivedDocument",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentRevokeId",
                table: "ArchivedDocument",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPartner",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "ArchivedDocument",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NumberOfDocument",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "ArchivedDocument",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sender",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedBy",
                table: "ArchivedDocument",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentTypeWorkflow",
                columns: table => new
                {
                    DocumentTypeWorkflowId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypeWorkflow", x => x.DocumentTypeWorkflowId);
                    table.ForeignKey(
                        name: "FK_DocumentTypeWorkflow_DocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTypeWorkflow_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flow",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FlowNumber = table.Column<int>(type: "integer", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flow", x => x.FlowId);
                    table.ForeignKey(
                        name: "FK_Flow_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypeWorkflow_DocumentTypeId",
                table: "DocumentTypeWorkflow",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypeWorkflow_WorkflowId",
                table: "DocumentTypeWorkflow",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Flow_WorkflowId",
                table: "Flow",
                column: "WorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Step_Flow_FlowId",
                table: "Step",
                column: "FlowId",
                principalTable: "Flow",
                principalColumn: "FlowId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Step_Flow_FlowId",
                table: "Step");

            migrationBuilder.DropTable(
                name: "DocumentTypeWorkflow");

            migrationBuilder.DropTable(
                name: "Flow");

            migrationBuilder.DropColumn(
                name: "IsAllocate",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Workflow");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "User");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "TaskUser");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "TaskUser");

            migrationBuilder.DropColumn(
                name: "TaskUserStatus",
                table: "TaskUser");

            migrationBuilder.DropColumn(
                name: "DateIssued",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DateReceived",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DocumentContent",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "NumberOfDocument",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "ArchivedDocumentContent",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "ArchivedDocumentStatus",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "DateIssued",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "DateReceived",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "DateSented",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "DocumentReplaceId",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "DocumentRevokeId",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "ExternalPartner",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "NumberOfDocument",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Sender",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "SignedBy",
                table: "ArchivedDocument");

            migrationBuilder.RenameColumn(
                name: "FlowId",
                table: "Step",
                newName: "WorkflowId");

            migrationBuilder.RenameIndex(
                name: "IX_Step_FlowId",
                table: "Step",
                newName: "IX_Step_WorkflowId");

            migrationBuilder.RenameColumn(
                name: "SignedBy",
                table: "Document",
                newName: "DocumentNumber");

            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "Document",
                newName: "DocumentCode");

            migrationBuilder.RenameColumn(
                name: "ProcessingStatus",
                table: "Document",
                newName: "DocumentStatus");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentTypeId",
                table: "Workflow",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentFileExtensionId",
                table: "Document",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Document",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Document",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Deadline",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Deadline",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Deadline",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentFileExtensionId",
                table: "AttachmentDocument",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DocumentFileExtension",
                columns: table => new
                {
                    DocumentFileExtensionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DocumentFileExtensionName = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFileExtension", x => x.DocumentFileExtensionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_DocumentTypeId",
                table: "Workflow",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentFileExtensionId",
                table: "Document",
                column: "DocumentFileExtensionId");

            migrationBuilder.CreateIndex(
                name: "IX_Deadline_UserId",
                table: "Deadline",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentDocument_DocumentFileExtensionId",
                table: "AttachmentDocument",
                column: "DocumentFileExtensionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentDocument_DocumentFileExtension_DocumentFileExtens~",
                table: "AttachmentDocument",
                column: "DocumentFileExtensionId",
                principalTable: "DocumentFileExtension",
                principalColumn: "DocumentFileExtensionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deadline_User_UserId",
                table: "Deadline",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentFileExtension_DocumentFileExtensionId",
                table: "Document",
                column: "DocumentFileExtensionId",
                principalTable: "DocumentFileExtension",
                principalColumn: "DocumentFileExtensionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Step_Workflow_WorkflowId",
                table: "Step",
                column: "WorkflowId",
                principalTable: "Workflow",
                principalColumn: "WorkflowId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workflow_DocumentType_DocumentTypeId",
                table: "Workflow",
                column: "DocumentTypeId",
                principalTable: "DocumentType",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
