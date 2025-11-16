using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class SetNull_CertificateTemplateFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiayChungNhan_MauGiayChungNhan_MaMau",
                table: "GiayChungNhan");

            migrationBuilder.AlterColumn<int>(
                name: "MaMau",
                table: "GiayChungNhan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GiayChungNhan_MauGiayChungNhan_MaMau",
                table: "GiayChungNhan",
                column: "MaMau",
                principalTable: "MauGiayChungNhan",
                principalColumn: "MaMau",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiayChungNhan_MauGiayChungNhan_MaMau",
                table: "GiayChungNhan");

            migrationBuilder.AlterColumn<int>(
                name: "MaMau",
                table: "GiayChungNhan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GiayChungNhan_MauGiayChungNhan_MaMau",
                table: "GiayChungNhan",
                column: "MaMau",
                principalTable: "MauGiayChungNhan",
                principalColumn: "MaMau",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
