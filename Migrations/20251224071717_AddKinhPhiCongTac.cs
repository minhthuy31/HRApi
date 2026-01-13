using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class AddKinhPhiCongTac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KinhPhiDuKien",
                table: "DangKyCongTacs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LyDoTamUng",
                table: "DangKyCongTacs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienTamUng",
                table: "DangKyCongTacs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KinhPhiDuKien",
                table: "DangKyCongTacs");

            migrationBuilder.DropColumn(
                name: "LyDoTamUng",
                table: "DangKyCongTacs");

            migrationBuilder.DropColumn(
                name: "SoTienTamUng",
                table: "DangKyCongTacs");
        }
    }
}
