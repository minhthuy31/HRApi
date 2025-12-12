using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class upbangl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BangLuongs_NhanViens_MaNhanVien",
                table: "BangLuongs");

            migrationBuilder.DropIndex(
                name: "IX_BangLuongs_MaNhanVien",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "DaChot",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "BangLuongs");

            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "BangLuongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "BangLuongs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "DaChot",
                table: "BangLuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "BangLuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BangLuongs_MaNhanVien",
                table: "BangLuongs",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_BangLuongs_NhanViens_MaNhanVien",
                table: "BangLuongs",
                column: "MaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
