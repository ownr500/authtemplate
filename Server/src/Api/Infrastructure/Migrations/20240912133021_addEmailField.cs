using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class addEmailField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("db0a2bde-ae9b-44c8-be2f-17e353a17d1a"));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailNormalized",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("3cdb2533-8321-4b7d-968a-5a7532a5e3ec"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f"),
                columns: new[] { "Email", "EmailNormalized" },
                values: new object[] { "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailNormalized",
                table: "Users",
                column: "EmailNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailNormalized",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("3cdb2533-8321-4b7d-968a-5a7532a5e3ec"));

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailNormalized",
                table: "Users");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("db0a2bde-ae9b-44c8-be2f-17e353a17d1a"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }
    }
}
