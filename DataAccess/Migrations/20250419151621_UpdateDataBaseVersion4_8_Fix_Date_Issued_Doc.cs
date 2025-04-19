using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseVersion4_8_Fix_Date_Issued_Doc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""Document"" 
        ALTER COLUMN ""DateIssued"" TYPE timestamp with time zone 
        USING ""DateIssued""::timestamp with time zone;
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""Document"" 
        ALTER COLUMN ""DateIssued"" TYPE text 
        USING ""DateIssued""::text;
    ");
        }
    }
}
