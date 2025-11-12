using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddMoTaToLinhVucAndKyNang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "LinhVuc",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "KyNang",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "LinhVuc");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "KyNang");
        }
    }
}

