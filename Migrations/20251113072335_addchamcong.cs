using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class addchamcong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "ChamCongs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "GioCheckOut",
                table: "ChamCongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChamCongs_MaNhanVien",
                table: "ChamCongs",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_ChamCongs_NhanViens_MaNhanVien",
                table: "ChamCongs",
                column: "MaNhanVien",
                principalTable: "NhanViens",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChamCongs_NhanViens_MaNhanVien",
                table: "ChamCongs");

            migrationBuilder.DropIndex(
                name: "IX_ChamCongs_MaNhanVien",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "GioCheckOut",
                table: "ChamCongs");

            migrationBuilder.AlterColumn<string>(
                name: "MaNhanVien",
                table: "ChamCongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
