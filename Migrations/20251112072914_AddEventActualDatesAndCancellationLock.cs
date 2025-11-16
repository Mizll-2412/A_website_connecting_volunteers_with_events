using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddEventActualDatesAndCancellationLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDienRaBatDau",
                table: "SuKien",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDienRaKetThuc",
                table: "SuKien",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianKhoaHuy",
                table: "SuKien",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayDienRaBatDau",
                table: "SuKien");

            migrationBuilder.DropColumn(
                name: "NgayDienRaKetThuc",
                table: "SuKien");

            migrationBuilder.DropColumn(
                name: "ThoiGianKhoaHuy",
                table: "SuKien");
        }
    }
}
