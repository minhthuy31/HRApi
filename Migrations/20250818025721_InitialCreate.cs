using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ChucVuNhanViens",
                columns: table => new
                {
                    MaChucVuNV = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenChucVu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HSPC = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChucVuNhanViens", x => x.MaChucVuNV);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganhs",
                columns: table => new
                {
                    MaChuyenNganh = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenChuyenNganh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganhs", x => x.MaChuyenNganh);
                });

            migrationBuilder.CreateTable(
                name: "HopDongs",
                columns: table => new
                {
                    MaHopDong = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiHopDong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDongs", x => x.MaHopDong);
                });

            migrationBuilder.CreateTable(
                name: "PhongBans",
                columns: table => new
                {
                    MaPhongBan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenPhongBan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sdt_PhongBan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongBans", x => x.MaPhongBan);
                });

            migrationBuilder.CreateTable(
                name: "TrinhDoHocVans",
                columns: table => new
                {
                    MaTrinhDoHocVan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenTrinhDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeSoBac = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrinhDoHocVans", x => x.MaTrinhDoHocVan);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QueQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioiTinh = table.Column<int>(type: "int", nullable: true),
                    DanToc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sdt_NhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaChucVuNV = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    MaPhongBan = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaHopDong = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaChuyenNganh = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaTrinhDoHocVan = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.MaNhanVien);
                    table.ForeignKey(
                        name: "FK_NhanViens_ChucVuNhanViens_MaChucVuNV",
                        column: x => x.MaChucVuNV,
                        principalTable: "ChucVuNhanViens",
                        principalColumn: "MaChucVuNV");
                    table.ForeignKey(
                        name: "FK_NhanViens_ChuyenNganhs_MaChuyenNganh",
                        column: x => x.MaChuyenNganh,
                        principalTable: "ChuyenNganhs",
                        principalColumn: "MaChuyenNganh");
                    table.ForeignKey(
                        name: "FK_NhanViens_HopDongs_MaHopDong",
                        column: x => x.MaHopDong,
                        principalTable: "HopDongs",
                        principalColumn: "MaHopDong");
                    table.ForeignKey(
                        name: "FK_NhanViens_PhongBans_MaPhongBan",
                        column: x => x.MaPhongBan,
                        principalTable: "PhongBans",
                        principalColumn: "MaPhongBan");
                    table.ForeignKey(
                        name: "FK_NhanViens_TrinhDoHocVans_MaTrinhDoHocVan",
                        column: x => x.MaTrinhDoHocVan,
                        principalTable: "TrinhDoHocVans",
                        principalColumn: "MaTrinhDoHocVan");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaChucVuNV",
                table: "NhanViens",
                column: "MaChucVuNV");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaChuyenNganh",
                table: "NhanViens",
                column: "MaChuyenNganh");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaHopDong",
                table: "NhanViens",
                column: "MaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaPhongBan",
                table: "NhanViens",
                column: "MaPhongBan");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_MaTrinhDoHocVan",
                table: "NhanViens",
                column: "MaTrinhDoHocVan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "ChucVuNhanViens");

            migrationBuilder.DropTable(
                name: "ChuyenNganhs");

            migrationBuilder.DropTable(
                name: "HopDongs");

            migrationBuilder.DropTable(
                name: "PhongBans");

            migrationBuilder.DropTable(
                name: "TrinhDoHocVans");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");
        }
    }
}
