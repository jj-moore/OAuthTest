using Microsoft.EntityFrameworkCore.Migrations;

namespace JJM_ImageSystems.Data.Migrations
{
    public partial class addaccesstoken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "PowerSchool",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "PowerSchool");
        }
    }
}
