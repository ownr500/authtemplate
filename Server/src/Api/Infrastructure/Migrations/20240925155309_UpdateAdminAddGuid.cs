using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminAddGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f"),
                columns: new[] { "Email", "EmailNormalized" },
                values: new object[] { "admin@5994471abb01112afcc181.com", "admin@5994471abb01112afcc181.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f"),
                columns: new[] { "Email", "EmailNormalized" },
                values: new object[] { "", "" });
        }
    }
}
