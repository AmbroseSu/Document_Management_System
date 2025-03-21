using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDocumentPermission_Permission_PermissionId",
                table: "UserDocumentPermission");

            migrationBuilder.DropTable(
                name: "Deadline");

            migrationBuilder.DropTable(
                name: "TaskUser");

            migrationBuilder.DropTable(
                name: "UserDocument");

            migrationBuilder.DropIndex(
                name: "IX_VerificationOtp_UserId",
                table: "VerificationOtp");

            migrationBuilder.DropIndex(
                name: "IX_UserDocumentPermission_PermissionId",
                table: "UserDocumentPermission");

            migrationBuilder.DropIndex(
                name: "IX_Role_StepId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "UserDocumentPermission");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                table: "User");

            migrationBuilder.DropColumn(
                name: "VerificationOtpId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StepId",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "DeadlineId",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                table: "Document",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document",
                newName: "IX_Document_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Task",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Document",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentTypeWorkflowId",
                table: "Document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalArchiveDocumentId",
                table: "Document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateArchiveDocumentId",
                table: "Document",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalDocumentId",
                table: "ArchivedDocument",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DigitalCertificate",
                columns: table => new
                {
                    DigitalCertificateId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    Issuer = table.Column<string>(type: "text", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: true),
                    HashAlgorithm = table.Column<string>(type: "text", nullable: true),
                    OwnerName = table.Column<string>(type: "text", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureImageUrl = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalCertificate", x => x.DigitalCertificateId);
                    table.ForeignKey(
                        name: "FK_DigitalCertificate_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersion",
                columns: table => new
                {
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VersionNumber = table.Column<string>(type: "text", nullable: true),
                    DocumentVersionUrl = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFinalVersion = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersion", x => x.DocumentVersionId);
                    table.ForeignKey(
                        name: "FK_DocumentVersion_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveDocumentSignature",
                columns: table => new
                {
                    ArchiveDocumentSignatureId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    SignatureValue = table.Column<string>(type: "text", nullable: false),
                    DigitalCertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchivedDocumentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveDocumentSignature", x => x.ArchiveDocumentSignatureId);
                    table.ForeignKey(
                        name: "FK_ArchiveDocumentSignature_ArchivedDocument_ArchivedDocumentId",
                        column: x => x.ArchivedDocumentId,
                        principalTable: "ArchivedDocument",
                        principalColumn: "ArchivedDocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveDocumentSignature_DigitalCertificate_DigitalCertific~",
                        column: x => x.DigitalCertificateId,
                        principalTable: "DigitalCertificate",
                        principalColumn: "DigitalCertificateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSignature",
                columns: table => new
                {
                    DocumentSignatureId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    SignatureValue = table.Column<string>(type: "text", nullable: false),
                    DigitalCertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSignature", x => x.DocumentSignatureId);
                    table.ForeignKey(
                        name: "FK_DocumentSignature_DigitalCertificate_DigitalCertificateId",
                        column: x => x.DigitalCertificateId,
                        principalTable: "DigitalCertificate",
                        principalColumn: "DigitalCertificateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentSignature_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationOtp_UserId",
                table: "VerificationOtp",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_UserId",
                table: "Task",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Step_RoleId",
                table: "Step",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeWorkflowId",
                table: "Document",
                column: "DocumentTypeWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_FinalArchiveDocumentId",
                table: "Document",
                column: "FinalArchiveDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Document_TemplateArchiveDocumentId",
                table: "Document",
                column: "TemplateArchiveDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument",
                column: "DocumentReplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument",
                column: "DocumentRevokeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveDocumentSignature_ArchivedDocumentId",
                table: "ArchiveDocumentSignature",
                column: "ArchivedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveDocumentSignature_DigitalCertificateId",
                table: "ArchiveDocumentSignature",
                column: "DigitalCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalCertificate_UserId",
                table: "DigitalCertificate",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignature_DigitalCertificateId",
                table: "DocumentSignature",
                column: "DigitalCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignature_DocumentId",
                table: "DocumentSignature",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersion_DocumentId",
                table: "DocumentVersion",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedDocument_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument",
                column: "DocumentReplaceId",
                principalTable: "ArchivedDocument",
                principalColumn: "ArchivedDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedDocument_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument",
                column: "DocumentRevokeId",
                principalTable: "ArchivedDocument",
                principalColumn: "ArchivedDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ArchivedDocument_FinalArchiveDocumentId",
                table: "Document",
                column: "FinalArchiveDocumentId",
                principalTable: "ArchivedDocument",
                principalColumn: "ArchivedDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ArchivedDocument_TemplateArchiveDocumentId",
                table: "Document",
                column: "TemplateArchiveDocumentId",
                principalTable: "ArchivedDocument",
                principalColumn: "ArchivedDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentTypeWorkflow_DocumentTypeWorkflowId",
                table: "Document",
                column: "DocumentTypeWorkflowId",
                principalTable: "DocumentTypeWorkflow",
                principalColumn: "DocumentTypeWorkflowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_User_UserId",
                table: "Document",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Step_Role_RoleId",
                table: "Step",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_User_UserId",
                table: "Task",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedDocument_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedDocument_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ArchivedDocument_FinalArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ArchivedDocument_TemplateArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_DocumentTypeWorkflow_DocumentTypeWorkflowId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_User_UserId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Step_Role_RoleId",
                table: "Step");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_User_UserId",
                table: "Task");

            migrationBuilder.DropTable(
                name: "ArchiveDocumentSignature");

            migrationBuilder.DropTable(
                name: "DocumentSignature");

            migrationBuilder.DropTable(
                name: "DocumentVersion");

            migrationBuilder.DropTable(
                name: "DigitalCertificate");

            migrationBuilder.DropIndex(
                name: "IX_VerificationOtp_UserId",
                table: "VerificationOtp");

            migrationBuilder.DropIndex(
                name: "IX_Task_UserId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Step_RoleId",
                table: "Step");

            migrationBuilder.DropIndex(
                name: "IX_Document_DocumentTypeWorkflowId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_Document_FinalArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_Document_TemplateArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentReplaceId",
                table: "ArchivedDocument");

            migrationBuilder.DropIndex(
                name: "IX_ArchivedDocument_DocumentRevokeId",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DocumentTypeWorkflowId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FinalArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "TemplateArchiveDocumentId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FinalDocumentId",
                table: "ArchivedDocument");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Document",
                newName: "DocumentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_UserId",
                table: "Document",
                newName: "IX_Document_DocumentTypeId");

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "UserDocumentPermission",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SignatureImage",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationOtpId",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "StepId",
                table: "Role",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeadlineId",
                table: "Document",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Deadline",
                columns: table => new
                {
                    DeadlineId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deadline", x => x.DeadlineId);
                    table.ForeignKey(
                        name: "FK_Deadline_Document_DeadlineId",
                        column: x => x.DeadlineId,
                        principalTable: "Document",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskUser",
                columns: table => new
                {
                    TaskUserId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCreatedTaskByUser = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    TaskUserStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskUser", x => x.TaskUserId);
                    table.ForeignKey(
                        name: "FK_TaskUser_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskUser_Task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDocument",
                columns: table => new
                {
                    UserDocumentId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCreatedDocumentByUser = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocument", x => x.UserDocumentId);
                    table.ForeignKey(
                        name: "FK_UserDocument_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDocument_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationOtp_UserId",
                table: "VerificationOtp",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentPermission_PermissionId",
                table: "UserDocumentPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_StepId",
                table: "Role",
                column: "StepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_RoleId",
                table: "TaskUser",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_TaskId",
                table: "TaskUser",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_UserId",
                table: "TaskUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_DocumentId",
                table: "UserDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_UserId",
                table: "UserDocument",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DocumentType_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId",
                principalTable: "DocumentType",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Step_StepId",
                table: "Role",
                column: "StepId",
                principalTable: "Step",
                principalColumn: "StepId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDocumentPermission_Permission_PermissionId",
                table: "UserDocumentPermission",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
