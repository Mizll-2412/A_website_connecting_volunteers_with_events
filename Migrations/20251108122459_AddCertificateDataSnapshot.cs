using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateDataSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateData",
                table: "GiayChungNhan",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateData",
                table: "GiayChungNhan");
        }
    }
}
