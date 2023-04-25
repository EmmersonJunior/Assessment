using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Theoremone.SmartAc.Data.Migrations.Sqlite
{
    public partial class device_alert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceAlerts",
                columns: table => new
                {
                    DeviceAlertId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlertStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertType = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertDescription = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceReadingId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAlerts", x => x.DeviceAlertId);
                    table.ForeignKey(
                        name: "FK_DeviceAlerts_DeviceReadings_DeviceReadingId",
                        column: x => x.DeviceReadingId,
                        principalTable: "DeviceReadings",
                        principalColumn: "DeviceReadingId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlerts_DeviceReadingId",
                table: "DeviceAlerts",
                column: "DeviceReadingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAlerts");
        }
    }
}
