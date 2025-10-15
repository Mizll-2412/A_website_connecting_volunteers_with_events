using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddDiemTrungBinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiemTrungBinh",
                table: "ToChuc",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoTuChoi",
                table: "ToChuc",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte>(
                name: "TrangThaiXacMinh",
                table: "ToChuc",
                type: "tinyint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemTrungBinh",
                table: "TinhNguyenVien",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TrangThai",
                table: "DonDangKy",
                type: "int",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint unsigned",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemTrungBinh",
                table: "ToChuc");

            migrationBuilder.DropColumn(
                name: "LyDoTuChoi",
                table: "ToChuc");

            migrationBuilder.DropColumn(
                name: "TrangThaiXacMinh",
                table: "ToChuc");

            migrationBuilder.DropColumn(
                name: "DiemTrungBinh",
                table: "TinhNguyenVien");

            migrationBuilder.AlterColumn<byte>(
                name: "TrangThai",
                table: "DonDangKy",
                type: "tinyint unsigned",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
