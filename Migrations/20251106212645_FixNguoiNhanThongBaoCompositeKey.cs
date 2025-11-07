using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class FixNguoiNhanThongBaoCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiNhanThongBao",
                table: "NguoiNhanThongBao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiNhanThongBao",
                table: "NguoiNhanThongBao",
                columns: new[] { "MaNguoiNhanThongBao", "MaThongBao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NguoiNhanThongBao",
                table: "NguoiNhanThongBao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NguoiNhanThongBao",
                table: "NguoiNhanThongBao",
                column: "MaNguoiNhanThongBao");
        }
    }
}
