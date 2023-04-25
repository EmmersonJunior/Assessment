using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Theoremone.SmartAc.Data.Migrations.Sqlite
{
    public partial class alert_date_time : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceReadingDate",
                table: "DeviceAlerts",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceReadingDate",
                table: "DeviceAlerts");
        }
    }
}
