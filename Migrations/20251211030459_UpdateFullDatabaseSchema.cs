using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFullDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "HopDongs");

            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "HopDongs",
                newName: "NgayHetHan");

            migrationBuilder.RenameColumn(
                name: "LuongThucNhan",
                table: "BangLuongs",
                newName: "TongThuNhap");

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "TrinhDoHocVans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChuyenNganhChiTiet",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChiKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeDaoTao",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaQuanLyTrucTiep",
                table: "NhanViens",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapHoChieu",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHanCCCD",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHanHoChieu",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayNghiViec",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayVaoLam",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiLienHeKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiCapHoChieu",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDKKCB",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDaoTao",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiSinh",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhuongXaThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuanHeKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuanHuyenThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuocGiaThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuocTich",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SdtKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoBHXH",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoBHYT",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoHoChieu",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenTaiKhoanNH",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhThanhThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TonGiao",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LuongDongBaoHiem",
                table: "HopDongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHieuLuc",
                table: "HopDongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKy",
                table: "HopDongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<string>(
                name: "MoTaCongViec",
                table: "ChucVuNhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiMuon",
                table: "ChamCongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "GioCheckIn",
                table: "ChamCongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiNgayCong",
                table: "ChamCongs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "SoGioLamViec",
                table: "ChamCongs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SoGioOT",
                table: "ChamCongs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "VeSom",
                table: "ChamCongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<decimal>(
                name: "KhauTruBHTN",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KhauTruBHXH",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KhauTruBHYT",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KhoanTruKhac",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LuongChinh",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LuongDongBaoHiem",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LuongOT",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThucLanh",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThueTNCN",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "TongGioOT",
                table: "BangLuongs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "TongPhuCap",
                table: "BangLuongs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaQuanLyTrucTiep",
                table: "NhanViens",
                column: "MaQuanLyTrucTiep");

            migrationBuilder.CreateIndex(
                name: "IX_ChucVuNhanViens_RoleId",
                table: "ChucVuNhanViens",
                column: "RoleId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ChucVuNhanViens_UserRoles_RoleId",
                table: "ChucVuNhanViens",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanViens_NhanViens_MaQuanLyTrucTiep",
                table: "NhanViens",
                column: "MaQuanLyTrucTiep",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BangLuongs_NhanViens_MaNhanVien",
                table: "BangLuongs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChucVuNhanViens_UserRoles_RoleId",
                table: "ChucVuNhanViens");

            migrationBuilder.DropForeignKey(
                name: "FK_NhanViens_NhanViens_MaQuanLyTrucTiep",
                table: "NhanViens");

            migrationBuilder.DropIndex(
                name: "IX_NhanViens_MaQuanLyTrucTiep",
                table: "NhanViens");

            migrationBuilder.DropIndex(
                name: "IX_ChucVuNhanViens_RoleId",
                table: "ChucVuNhanViens");

            migrationBuilder.DropIndex(
                name: "IX_BangLuongs_MaNhanVien",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "TrinhDoHocVans");

            migrationBuilder.DropColumn(
                name: "ChuyenNganhChiTiet",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "DiaChiKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "HeDaoTao",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "MaQuanLyTrucTiep",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayCapHoChieu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayHetHanCCCD",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayHetHanHoChieu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayNghiViec",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayVaoLam",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NguoiLienHeKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiCapHoChieu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiDKKCB",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiDaoTao",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiSinh",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "PhuongXaThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "QuanHeKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "QuanHuyenThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "QuocGiaThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "QuocTich",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SdtKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SoBHXH",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SoBHYT",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SoHoChieu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "TenTaiKhoanNH",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "TinhThanhThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "TonGiao",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "LuongDongBaoHiem",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "NgayHieuLuc",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "NgayKy",
                table: "HopDongs");

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

            migrationBuilder.DropColumn(
                name: "MoTaCongViec",
                table: "ChucVuNhanViens");

            migrationBuilder.DropColumn(
                name: "DiMuon",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "GioCheckIn",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "LoaiNgayCong",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "SoGioLamViec",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "SoGioOT",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "VeSom",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "DaChot",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "KhauTruBHTN",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "KhauTruBHXH",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "KhauTruBHYT",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "KhoanTruKhac",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "LuongChinh",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "LuongDongBaoHiem",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "LuongOT",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "ThucLanh",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "ThueTNCN",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "TongGioOT",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "TongPhuCap",
                table: "BangLuongs");

            migrationBuilder.RenameColumn(
                name: "NgayHetHan",
                table: "HopDongs",
                newName: "NgayKetThuc");

            migrationBuilder.RenameColumn(
                name: "TongThuNhap",
                table: "BangLuongs",
                newName: "LuongThucNhan");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "HopDongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "BangLuongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
