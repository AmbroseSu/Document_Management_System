using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameTableDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Resource_ResourceId",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermission",
                table: "RolePermission");

            migrationBuilder.RenameTable(
                name: "RolePermission",
                newName: "RoleResource");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermission_RoleId",
                table: "RoleResource",
                newName: "IX_RoleResource_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermission_ResourceId",
                table: "RoleResource",
                newName: "IX_RoleResource_ResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleResource",
                table: "RoleResource",
                column: "RoleResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleResource_Resource_ResourceId",
                table: "RoleResource",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "ResourceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleResource_Role_RoleId",
                table: "RoleResource",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleResource_Resource_ResourceId",
                table: "RoleResource");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleResource_Role_RoleId",
                table: "RoleResource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleResource",
                table: "RoleResource");

            migrationBuilder.RenameTable(
                name: "RoleResource",
                newName: "RolePermission");

            migrationBuilder.RenameIndex(
                name: "IX_RoleResource_RoleId",
                table: "RolePermission",
                newName: "IX_RolePermission_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleResource_ResourceId",
                table: "RolePermission",
                newName: "IX_RolePermission_ResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermission",
                table: "RolePermission",
                column: "RoleResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Resource_ResourceId",
                table: "RolePermission",
                column: "ResourceId",
                principalTable: "Resource",
                principalColumn: "ResourceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                table: "RolePermission",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
