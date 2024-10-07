using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class recoveryTokenUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("82a7e1ec-821e-4cf1-9028-d50c6e15f437"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RecoveryTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("7d716d2b-c0cf-44c8-b52c-cf834f8b7775"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("7d716d2b-c0cf-44c8-b52c-cf834f8b7775"));

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RecoveryTokens");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("82a7e1ec-821e-4cf1-9028-d50c6e15f437"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }
    }
}
