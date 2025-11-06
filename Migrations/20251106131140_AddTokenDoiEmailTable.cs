using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenDoiEmailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MauGiayChungNhan_SuKien_MaSuKien",
                table: "MauGiayChungNhan");

            migrationBuilder.AlterColumn<int>(
                name: "MaSuKien",
                table: "MauGiayChungNhan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "MauGiayChungNhan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "MauGiayChungNhan",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenMau",
                table: "MauGiayChungNhan",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaSuKien",
                table: "GiayChungNhan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TokenDoiEmail",
                columns: table => new
                {
                    MaToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    EmailMoi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenDoiEmail", x => x.MaToken);
                    table.ForeignKey(
                        name: "FK_TokenDoiEmail_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiayChungNhan_MaSuKien",
                table: "GiayChungNhan",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_TokenDoiEmail_MaTaiKhoan",
                table: "TokenDoiEmail",
                column: "MaTaiKhoan");

            migrationBuilder.AddForeignKey(
                name: "FK_GiayChungNhan_SuKien_MaSuKien",
                table: "GiayChungNhan",
                column: "MaSuKien",
                principalTable: "SuKien",
                principalColumn: "MaSuKien",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MauGiayChungNhan_SuKien_MaSuKien",
                table: "MauGiayChungNhan",
                column: "MaSuKien",
                principalTable: "SuKien",
                principalColumn: "MaSuKien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiayChungNhan_SuKien_MaSuKien",
                table: "GiayChungNhan");

            migrationBuilder.DropForeignKey(
                name: "FK_MauGiayChungNhan_SuKien_MaSuKien",
                table: "MauGiayChungNhan");

            migrationBuilder.DropTable(
                name: "TokenDoiEmail");

            migrationBuilder.DropIndex(
                name: "IX_GiayChungNhan_MaSuKien",
                table: "GiayChungNhan");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "TenMau",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "MaSuKien",
                table: "GiayChungNhan");

            migrationBuilder.AlterColumn<int>(
                name: "MaSuKien",
                table: "MauGiayChungNhan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MauGiayChungNhan_SuKien_MaSuKien",
                table: "MauGiayChungNhan",
                column: "MaSuKien",
                principalTable: "SuKien",
                principalColumn: "MaSuKien",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
