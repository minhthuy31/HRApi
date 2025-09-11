using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleTableAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "NhanViens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameRole = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.RoleId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_RoleId",
                table: "NhanViens",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanViens_UserRoles_RoleId",
                table: "NhanViens",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NhanViens_UserRoles_RoleId",
                table: "NhanViens");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_NhanViens_RoleId",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "NhanViens");
        }
    }
}
