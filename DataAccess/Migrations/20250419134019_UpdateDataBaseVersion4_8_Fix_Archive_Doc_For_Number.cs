using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion4_8_Fix_Archive_Doc_For_Number : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Llx",
                table: "ArchivedDocument",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Lly",
                table: "ArchivedDocument",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Page",
                table: "ArchivedDocument",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Urx",
                table: "ArchivedDocument",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ury",
                table: "ArchivedDocument",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Llx",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Lly",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Page",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Urx",
                table: "ArchivedDocument");

            migrationBuilder.DropColumn(
                name: "Ury",
                table: "ArchivedDocument");
        }
    }
}
