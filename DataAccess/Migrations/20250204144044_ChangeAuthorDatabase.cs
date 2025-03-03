using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAuthorDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Permission_PermissionId",
                table: "RolePermission");

            migrationBuilder.DropTable(
                name: "ResourcePermission");

            migrationBuilder.DropColumn(
                name: "ResourceMethod",
                table: "Resource");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "RolePermission",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "RolePermissionId",
                table: "RolePermission",
                newName: "RoleResourceId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                newName: "IX_RolePermission_ResourceId");

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "Resource",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Resource_PermissionId",
                table: "Resource",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resource_Permission_PermissionId",
                table: "Resource",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Resource_ResourceId",
                table: "RolePermission",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "ResourceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resource_Permission_PermissionId",
                table: "Resource");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Resource_ResourceId",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_Resource_PermissionId",
                table: "Resource");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "Resource");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "RolePermission",
                newName: "PermissionId");

            migrationBuilder.RenameColumn(
                name: "RoleResourceId",
                table: "RolePermission",
                newName: "RolePermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermission_ResourceId",
                table: "RolePermission",
                newName: "IX_RolePermission_PermissionId");

            migrationBuilder.AddColumn<string>(
                name: "ResourceMethod",
                table: "Resource",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResourcePermission",
                columns: table => new
                {
                    ResourcePermissionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolePermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePermission", x => x.ResourcePermissionId);
                    table.ForeignKey(
                        name: "FK_ResourcePermission_Resource_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resource",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourcePermission_RolePermission_RolePermissionId",
                        column: x => x.RolePermissionId,
                        principalTable: "RolePermission",
                        principalColumn: "RolePermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePermission_ResourceId",
                table: "ResourcePermission",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourcePermission_RolePermissionId",
                table: "ResourcePermission",
                column: "RolePermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Permission_PermissionId",
                table: "RolePermission",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
