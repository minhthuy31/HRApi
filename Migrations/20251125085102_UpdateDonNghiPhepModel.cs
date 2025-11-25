using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonNghiPhepModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NgayNghi",
                table: "DonNghiPheps",
                newName: "NgayKetThuc");

            migrationBuilder.AlterColumn<string>(
                name: "LyDo",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDau",
                table: "DonNghiPheps",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "SoNgayNghi",
                table: "DonNghiPheps",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TepDinhKem",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "DonNghiPheps");

            migrationBuilder.DropColumn(
                name: "SoNgayNghi",
                table: "DonNghiPheps");

            migrationBuilder.DropColumn(
                name: "TepDinhKem",
                table: "DonNghiPheps");

            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "DonNghiPheps",
                newName: "NgayNghi");

            migrationBuilder.AlterColumn<string>(
                name: "LyDo",
                table: "DonNghiPheps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
