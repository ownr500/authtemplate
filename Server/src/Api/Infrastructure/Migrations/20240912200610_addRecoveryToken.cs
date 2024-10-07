using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class addRecoveryToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("3cdb2533-8321-4b7d-968a-5a7532a5e3ec"));

            migrationBuilder.CreateTable(
                name: "RecoveryTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecoveryToken = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    RecoveryTokenExpireAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryTokens", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("82a7e1ec-821e-4cf1-9028-d50c6e15f437"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });

            migrationBuilder.CreateIndex(
                name: "IX_RecoveryTokens_RecoveryToken",
                table: "RecoveryTokens",
                column: "RecoveryToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecoveryTokens");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("82a7e1ec-821e-4cf1-9028-d50c6e15f437"));

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { new Guid("3cdb2533-8321-4b7d-968a-5a7532a5e3ec"), new Guid("c9a36382-bb77-4ee7-8539-681026b43916"), new Guid("561bbfaa-c44a-45f9-97c4-7182ba38b85f") });
        }
    }
}
