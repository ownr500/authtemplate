using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class recoveryTokenUpdateRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("7d716d2b-c0cf-44c8-b52c-cf834f8b7775"));

            migrationBuilder.RenameColumn(
                name: "RecoveryTokenExpireAt",
                table: "RecoveryTokens",
                newName: "ExpireAt");

            migrationBuilder.RenameColumn(
                name: "RecoveryToken",
                table: "RecoveryTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_RecoveryTokens_RecoveryToken",
                table: "RecoveryTokens",
                newName: "IX_RecoveryTokens_Token");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("1ff798f3-a2b2-4cd6-86f2-165d4fed054d"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("1ff798f3-a2b2-4cd6-86f2-165d4fed054d"));

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RecoveryTokens",
                newName: "RecoveryToken");

            migrationBuilder.RenameColumn(
                name: "ExpireAt",
                table: "RecoveryTokens",
                newName: "RecoveryTokenExpireAt");

            migrationBuilder.RenameIndex(
                name: "IX_RecoveryTokens_Token",
                table: "RecoveryTokens",
                newName: "IX_RecoveryTokens_RecoveryToken");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("7d716d2b-c0cf-44c8-b52c-cf834f8b7775"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }
    }
}
