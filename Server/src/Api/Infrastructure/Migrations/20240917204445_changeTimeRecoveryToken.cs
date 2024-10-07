using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class changeTimeRecoveryToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("1ff798f3-a2b2-4cd6-86f2-165d4fed054d"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpireAt",
                table: "RecoveryTokens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("7c8a2d3d-b820-4fa9-8dc8-b8c25b6c65fe"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("7c8a2d3d-b820-4fa9-8dc8-b8c25b6c65fe"));

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpireAt",
                table: "RecoveryTokens",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("1ff798f3-a2b2-4cd6-86f2-165d4fed054d"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }
    }
}
