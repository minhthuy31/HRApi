using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChamCongModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GioRa",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "GioVao",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "ChamCongs");

            migrationBuilder.AddColumn<double>(
                name: "NgayCong",
                table: "ChamCongs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCong",
                table: "ChamCongs");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioRa",
                table: "ChamCongs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioVao",
                table: "ChamCongs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "ChamCongs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
