using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateTemplateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImage",
                table: "MauGiayChungNhan",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "MauGiayChungNhan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TemplateConfig",
                table: "MauGiayChungNhan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "MauGiayChungNhan",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImage",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "TemplateConfig",
                table: "MauGiayChungNhan");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "MauGiayChungNhan");
        }
    }
}
