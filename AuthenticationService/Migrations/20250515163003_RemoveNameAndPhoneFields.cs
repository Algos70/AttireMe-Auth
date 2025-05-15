using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameAndPhoneFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2781efb3-5e0a-48ff-8291-39fedfaee776");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4fb2c75b-7d45-4ad0-9f23-732eb0b0a4e8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f5f865d9-1202-4576-9e41-493f2f7adecd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "26b6a407-fcb4-4ea0-93c5-dbcb2099324f", "1", "Admin", "ADMIN" },
                    { "76275413-9a0b-49ec-a0f3-e8c867d07912", "2", "Creator", "CREATOR" },
                    { "95a6013a-a617-470a-8184-f6028bfd07a0", "3", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "26b6a407-fcb4-4ea0-93c5-dbcb2099324f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "76275413-9a0b-49ec-a0f3-e8c867d07912");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "95a6013a-a617-470a-8184-f6028bfd07a0");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2781efb3-5e0a-48ff-8291-39fedfaee776", "1", "Admin", "ADMIN" },
                    { "4fb2c75b-7d45-4ad0-9f23-732eb0b0a4e8", "2", "Creator", "CREATOR" },
                    { "f5f865d9-1202-4576-9e41-493f2f7adecd", "3", "User", "USER" }
                });
        }
    }
}
