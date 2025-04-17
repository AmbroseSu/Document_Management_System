using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion3_3_Update_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""WorkflowFlow"" 
          ALTER COLUMN ""FlowNumber"" TYPE integer 
          USING CASE 
              WHEN ""FlowNumber"" = true THEN 1 
              ELSE 0 
          END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""WorkflowFlow"" 
          ALTER COLUMN ""FlowNumber"" TYPE boolean 
          USING CASE 
              WHEN ""FlowNumber"" = 1 THEN true 
              ELSE false 
          END");
        }
    }
}
