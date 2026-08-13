using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinderStore.Backend.Infrastructure.Migrations
{
    public partial class AddBackupPhoneToApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackupPhone",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupPhone",
                table: "AspNetUsers");
        }
    }
}
