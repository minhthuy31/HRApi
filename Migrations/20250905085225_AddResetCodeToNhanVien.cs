using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class AddResetCodeToNhanVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetCode",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetCodeExpiry",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetCode",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "ResetCodeExpiry",
                table: "NhanViens");
        }
    }
}
