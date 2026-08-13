using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinderStore.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addnewprofilecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackupPhone",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupPhone",
                table: "AspNetUsers");
        }
    }
}
