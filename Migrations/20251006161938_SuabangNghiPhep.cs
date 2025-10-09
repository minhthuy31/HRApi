using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class SuabangNghiPhep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonNghiPheps_NhanViens_NhanVienMaNhanVien",
                table: "DonNghiPheps");

            migrationBuilder.DropIndex(
                name: "IX_DonNghiPheps_NhanVienMaNhanVien",
                table: "DonNghiPheps");

            migrationBuilder.DropColumn(
                name: "NhanVienMaNhanVien",
                table: "DonNghiPheps");

            migrationBuilder.RenameColumn(
                name: "NgayGui",
                table: "DonNghiPheps",
                newName: "NgayGuiDon");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "DonNghiPheps",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_DonNghiPheps_MaNhanVien",
                table: "DonNghiPheps",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_DonNghiPheps_NhanViens_MaNhanVien",
                table: "DonNghiPheps",
                column: "MaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonNghiPheps_NhanViens_MaNhanVien",
                table: "DonNghiPheps");

            migrationBuilder.DropIndex(
                name: "IX_DonNghiPheps_MaNhanVien",
                table: "DonNghiPheps");

            migrationBuilder.RenameColumn(
                name: "NgayGuiDon",
                table: "DonNghiPheps",
                newName: "NgayGui");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NhanVienMaNhanVien",
                table: "DonNghiPheps",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonNghiPheps_NhanVienMaNhanVien",
                table: "DonNghiPheps",
                column: "NhanVienMaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_DonNghiPheps_NhanViens_NhanVienMaNhanVien",
                table: "DonNghiPheps",
                column: "NhanVienMaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien");
        }
    }
}
