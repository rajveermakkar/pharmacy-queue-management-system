using Microsoft.EntityFrameworkCore.Migrations;

namespace PharmacyQueue.Data.Migrations
{
    public partial class AddDefaultAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Name", "Email", "Password", "Role", "IsActive", "CreatedDate", "LastLogin" },
                values: new object[] {
                    "Admin",
                    "admin@pharmacy.com",
                    "Admin@123",
                    "Administrator",
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Email",
                keyValue: "admin@pharmacy.com");
        }
    }
} 