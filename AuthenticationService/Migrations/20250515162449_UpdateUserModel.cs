using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "17240a90-0f1e-4a02-91f8-e916bee22d4a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "789955ce-1cb3-4384-a1cf-eccad41b5961");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d55f186d-4dc0-4a04-ae42-36df2078793e");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                    { "17240a90-0f1e-4a02-91f8-e916bee22d4a", "2", "Creator", "CREATOR" },
                    { "789955ce-1cb3-4384-a1cf-eccad41b5961", "1", "Admin", "ADMIN" },
                    { "d55f186d-4dc0-4a04-ae42-36df2078793e", "3", "User", "USER" }
                });
        }
    }
}
