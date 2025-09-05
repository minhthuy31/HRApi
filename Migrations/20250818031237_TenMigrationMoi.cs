using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class TenMigrationMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NhanViens_HopDongs_MaHopDong",
                table: "NhanViens");

            migrationBuilder.DropIndex(
                name: "IX_NhanViens_MaHopDong",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "MaHopDong",
                table: "NhanViens");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiTamTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChiThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiNhanVien",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapCCCD",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiCapCCCD",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoTaiKhoanNH",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenNganHang",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhTrangHonNhan",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaNhanVien",
                table: "HopDongs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongs_MaNhanVien",
                table: "HopDongs",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_HopDongs_NhanViens_MaNhanVien",
                table: "HopDongs",
                column: "MaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HopDongs_NhanViens_MaNhanVien",
                table: "HopDongs");

            migrationBuilder.DropIndex(
                name: "IX_HopDongs_MaNhanVien",
                table: "HopDongs");

            migrationBuilder.DropColumn(
                name: "DiaChiTamTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "DiaChiThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "LoaiNhanVien",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayCapCCCD",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiCapCCCD",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SoTaiKhoanNH",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "TenNganHang",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "TinhTrangHonNhan",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "MaNhanVien",
                table: "HopDongs");

            migrationBuilder.AddColumn<string>(
                name: "MaHopDong",
                table: "NhanViens",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaHopDong",
                table: "NhanViens",
                column: "MaHopDong");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanViens_HopDongs_MaHopDong",
                table: "NhanViens",
                column: "MaHopDong",
                principalTable: "HopDongs",
                principalColumn: "MaHopDong");
        }
    }
}
