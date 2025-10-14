using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace khoaluantotnghiep.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KyNang",
                columns: table => new
                {
                    MaKyNang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenKyNang = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyNang", x => x.MaKyNang);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LinhVuc",
                columns: table => new
                {
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenLinhVuc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhVuc", x => x.MaLinhVuc);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordSalt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VaiTro = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrangThai = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    MaAdmin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CCCD = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoDienThoai = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaChi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnhDaiDien = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.MaAdmin);
                    table.ForeignKey(
                        name: "FK_Admin_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    PhanLoai = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayGui = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TinhNguyenVien",
                columns: table => new
                {
                    MaTNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    GioiTinh = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CCCD = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaChi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GioiThieu = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnhDaiDien = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhNguyenVien", x => x.MaTNV);
                    table.ForeignKey(
                        name: "FK_TinhNguyenVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ToChuc",
                columns: table => new
                {
                    MaToChuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    TenToChuc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoDienThoai = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaChi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    GioiThieu = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnhDaiDien = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToChuc", x => x.MaToChuc);
                    table.ForeignKey(
                        name: "FK_ToChuc_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NguoiNhanThongBao",
                columns: table => new
                {
                    MaNguoiNhanThongBao = table.Column<int>(type: "int", nullable: false),
                    MaThongBao = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiNhanThongBao", x => x.MaNguoiNhanThongBao);
                    table.ForeignKey(
                        name: "FK_NguoiNhanThongBao_TaiKhoan_MaNguoiNhanThongBao",
                        column: x => x.MaNguoiNhanThongBao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NguoiNhanThongBao_ThongBao_MaThongBao",
                        column: x => x.MaThongBao,
                        principalTable: "ThongBao",
                        principalColumn: "MaThongBao",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TinhNguyenVien_KyNang",
                columns: table => new
                {
                    MaTNV = table.Column<int>(type: "int", nullable: false),
                    MaKyNang = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhNguyenVien_KyNang", x => new { x.MaTNV, x.MaKyNang });
                    table.ForeignKey(
                        name: "FK_TinhNguyenVien_KyNang_KyNang_MaKyNang",
                        column: x => x.MaKyNang,
                        principalTable: "KyNang",
                        principalColumn: "MaKyNang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinhNguyenVien_KyNang_TinhNguyenVien_MaTNV",
                        column: x => x.MaTNV,
                        principalTable: "TinhNguyenVien",
                        principalColumn: "MaTNV",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TinhNguyenVien_LinhVuc",
                columns: table => new
                {
                    MaTNV = table.Column<int>(type: "int", nullable: false),
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinhNguyenVien_LinhVuc", x => new { x.MaTNV, x.MaLinhVuc });
                    table.ForeignKey(
                        name: "FK_TinhNguyenVien_LinhVuc_LinhVuc_MaLinhVuc",
                        column: x => x.MaLinhVuc,
                        principalTable: "LinhVuc",
                        principalColumn: "MaLinhVuc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TinhNguyenVien_LinhVuc_TinhNguyenVien_MaTNV",
                        column: x => x.MaTNV,
                        principalTable: "TinhNguyenVien",
                        principalColumn: "MaTNV",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiayToPhapLy",
                columns: table => new
                {
                    MaGiayTo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaToChuc = table.Column<int>(type: "int", nullable: false),
                    TenGiayTo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    File = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiayToPhapLy", x => x.MaGiayTo);
                    table.ForeignKey(
                        name: "FK_GiayToPhapLy_ToChuc_MaToChuc",
                        column: x => x.MaToChuc,
                        principalTable: "ToChuc",
                        principalColumn: "MaToChuc",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SuKien",
                columns: table => new
                {
                    MaSuKien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaToChuc = table.Column<int>(type: "int", nullable: false),
                    TenSuKien = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NoiDung = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    DiaChi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayBatDau = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayTuyen = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NgayKetThucTuyen = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien", x => x.MaSuKien);
                    table.ForeignKey(
                        name: "FK_SuKien_ToChuc_MaToChuc",
                        column: x => x.MaToChuc,
                        principalTable: "ToChuc",
                        principalColumn: "MaToChuc",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaNguoiDanhGia = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDuocDanhGia = table.Column<int>(type: "int", nullable: false),
                    MaSuKien = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_SuKien_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKien",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGia_TaiKhoan_MaNguoiDanhGia",
                        column: x => x.MaNguoiDanhGia,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGia_TaiKhoan_MaNguoiDuocDanhGia",
                        column: x => x.MaNguoiDuocDanhGia,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DonDangKy",
                columns: table => new
                {
                    MaTNV = table.Column<int>(type: "int", nullable: false),
                    MaSuKien = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GhiChu = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrangThai = table.Column<byte>(type: "tinyint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDangKy", x => x.MaTNV);
                    table.ForeignKey(
                        name: "FK_DonDangKy_SuKien_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKien",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonDangKy_TinhNguyenVien_MaTNV",
                        column: x => x.MaTNV,
                        principalTable: "TinhNguyenVien",
                        principalColumn: "MaTNV",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MauGiayChungNhan",
                columns: table => new
                {
                    MaMau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaSuKien = table.Column<int>(type: "int", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    File = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauGiayChungNhan", x => x.MaMau);
                    table.ForeignKey(
                        name: "FK_MauGiayChungNhan_SuKien_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKien",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SuKien_KyNang",
                columns: table => new
                {
                    MaSuKien = table.Column<int>(type: "int", nullable: false),
                    MaKyNang = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien_KyNang", x => new { x.MaSuKien, x.MaKyNang });
                    table.ForeignKey(
                        name: "FK_SuKien_KyNang_KyNang_MaKyNang",
                        column: x => x.MaKyNang,
                        principalTable: "KyNang",
                        principalColumn: "MaKyNang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuKien_KyNang_SuKien_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKien",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SuKien_LinhVuc",
                columns: table => new
                {
                    MaSuKien = table.Column<int>(type: "int", nullable: false),
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKien_LinhVuc", x => new { x.MaSuKien, x.MaLinhVuc });
                    table.ForeignKey(
                        name: "FK_SuKien_LinhVuc_LinhVuc_MaLinhVuc",
                        column: x => x.MaLinhVuc,
                        principalTable: "LinhVuc",
                        principalColumn: "MaLinhVuc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SuKien_LinhVuc_SuKien_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKien",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GiayChungNhan",
                columns: table => new
                {
                    MaGiayChungNhan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaMau = table.Column<int>(type: "int", nullable: false),
                    MaTNV = table.Column<int>(type: "int", nullable: false),
                    NgayCap = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    File = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiayChungNhan", x => x.MaGiayChungNhan);
                    table.ForeignKey(
                        name: "FK_GiayChungNhan_MauGiayChungNhan_MaMau",
                        column: x => x.MaMau,
                        principalTable: "MauGiayChungNhan",
                        principalColumn: "MaMau",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GiayChungNhan_TinhNguyenVien_MaTNV",
                        column: x => x.MaTNV,
                        principalTable: "TinhNguyenVien",
                        principalColumn: "MaTNV",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_MaTaiKhoan",
                table: "Admin",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaNguoiDanhGia",
                table: "DanhGia",
                column: "MaNguoiDanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaNguoiDuocDanhGia",
                table: "DanhGia",
                column: "MaNguoiDuocDanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaSuKien",
                table: "DanhGia",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_DonDangKy_MaSuKien",
                table: "DonDangKy",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_GiayChungNhan_MaMau",
                table: "GiayChungNhan",
                column: "MaMau");

            migrationBuilder.CreateIndex(
                name: "IX_GiayChungNhan_MaTNV",
                table: "GiayChungNhan",
                column: "MaTNV");

            migrationBuilder.CreateIndex(
                name: "IX_GiayToPhapLy_MaToChuc",
                table: "GiayToPhapLy",
                column: "MaToChuc");

            migrationBuilder.CreateIndex(
                name: "IX_MauGiayChungNhan_MaSuKien",
                table: "MauGiayChungNhan",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiNhanThongBao_MaThongBao",
                table: "NguoiNhanThongBao",
                column: "MaThongBao");

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_MaToChuc",
                table: "SuKien",
                column: "MaToChuc");

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_KyNang_MaKyNang",
                table: "SuKien_KyNang",
                column: "MaKyNang");

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_LinhVuc_MaLinhVuc",
                table: "SuKien_LinhVuc",
                column: "MaLinhVuc");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaTaiKhoan",
                table: "ThongBao",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_TinhNguyenVien_MaTaiKhoan",
                table: "TinhNguyenVien",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_TinhNguyenVien_KyNang_MaKyNang",
                table: "TinhNguyenVien_KyNang",
                column: "MaKyNang");

            migrationBuilder.CreateIndex(
                name: "IX_TinhNguyenVien_LinhVuc_MaLinhVuc",
                table: "TinhNguyenVien_LinhVuc",
                column: "MaLinhVuc");

            migrationBuilder.CreateIndex(
                name: "IX_ToChuc_MaTaiKhoan",
                table: "ToChuc",
                column: "MaTaiKhoan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "DonDangKy");

            migrationBuilder.DropTable(
                name: "GiayChungNhan");

            migrationBuilder.DropTable(
                name: "GiayToPhapLy");

            migrationBuilder.DropTable(
                name: "NguoiNhanThongBao");

            migrationBuilder.DropTable(
                name: "SuKien_KyNang");

            migrationBuilder.DropTable(
                name: "SuKien_LinhVuc");

            migrationBuilder.DropTable(
                name: "TinhNguyenVien_KyNang");

            migrationBuilder.DropTable(
                name: "TinhNguyenVien_LinhVuc");

            migrationBuilder.DropTable(
                name: "MauGiayChungNhan");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "KyNang");

            migrationBuilder.DropTable(
                name: "LinhVuc");

            migrationBuilder.DropTable(
                name: "TinhNguyenVien");

            migrationBuilder.DropTable(
                name: "SuKien");

            migrationBuilder.DropTable(
                name: "ToChuc");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
