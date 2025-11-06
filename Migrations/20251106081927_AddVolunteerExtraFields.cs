using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CapBac",
                table: "TinhNguyenVien",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "TinhNguyenVien",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TongSuKienThamGia",
                table: "TinhNguyenVien",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TokenResetMatKhau",
                columns: table => new
                {
                    MaToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenResetMatKhau", x => x.MaToken);
                    table.ForeignKey(
                        name: "FK_TokenResetMatKhau_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenResetMatKhau_MaTaiKhoan",
                table: "TokenResetMatKhau",
                column: "MaTaiKhoan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenResetMatKhau");

            migrationBuilder.DropColumn(
                name: "CapBac",
                table: "TinhNguyenVien");

            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "TinhNguyenVien");

            migrationBuilder.DropColumn(
                name: "TongSuKienThamGia",
                table: "TinhNguyenVien");
        }
    }
}
