using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class hopdong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhuCapAnTrua",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "PhuCapKhac",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "PhuCapTrachNhiem",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "ThoiHanHopDong",
                table: "HopDongs");

            migrationBuilder.RenameColumn(
                name: "NgayHieuLuc",
                table: "HopDongs",
                newName: "NgayBatDau");

            migrationBuilder.RenameColumn(
                name: "NgayHetHan",
                table: "HopDongs",
                newName: "NgayKetThuc");

            migrationBuilder.RenameColumn(
                name: "MaHopDong",
                table: "HopDongs",
                newName: "SoHopDong");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "HopDongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiHopDong",
                table: "HopDongs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TepDinhKem",
                table: "HopDongs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TepDinhKem",
                table: "HopDongs");

            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "HopDongs",
                newName: "NgayHetHan");

            migrationBuilder.RenameColumn(
                name: "NgayBatDau",
                table: "HopDongs",
                newName: "NgayHieuLuc");

            migrationBuilder.RenameColumn(
                name: "SoHopDong",
                table: "HopDongs",
                newName: "MaHopDong");

            migrationBuilder.AlterColumn<bool>(
                name: "TrangThai",
                table: "HopDongs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiHopDong",
                table: "HopDongs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "PhuCapAnTrua",
                table: "HopDongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PhuCapKhac",
                table: "HopDongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PhuCapTrachNhiem",
                table: "HopDongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ThoiHanHopDong",
                table: "HopDongs",
                type: "int",
                nullable: true);
        }
    }
}
