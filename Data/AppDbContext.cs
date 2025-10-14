using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<TaiKhoan> User { get; set; }
        public DbSet<TinhNguyenVien> Volunteer { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<ToChuc> Organization { get; set; }
        public DbSet<SuKien> Event { get; set; }
        public DbSet<TinhNguyenVien_KyNang> TinhNguyenVien_KyNang { get; set; }
        public DbSet<TinhNguyenVien_LinhVuc> TinhNguyenVien_LinhVuc { get; set; }
        public DbSet<KyNang> KyNang { get; set; }
        public DbSet<DonDangKy> DonDangKy { get; set; }
        public DbSet<NguoiNhanThongBao> NguoiNhanThongBao { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }
        public DbSet<GiayToPhapLy> GiayToPhapLy { get; set; }
        public DbSet<LinhVuc> LinhVuc { get; set; }
        public DbSet<SuKien_KyNang> SuKien_KyNang { get; set; }
        public DbSet<SuKien_LinhVuc> SuKien_LinhVuc { get; set; }
        public DbSet<DanhGia> DanhGia { get; set; }
        public DbSet<MauGiayChungNhan> MauGiayChungNhan { get; set; }
        public DbSet<GiayChungNhan> GiayChungNhan { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình composite key cho các bảng N–N
            modelBuilder.Entity<SuKien_KyNang>()
                .HasKey(sk => new { sk.MaSuKien, sk.MaKyNang });

            modelBuilder.Entity<TinhNguyenVien_LinhVuc>()
                .HasKey(tk => new { tk.MaTNV, tk.MaLinhVuc });

            modelBuilder.Entity<SuKien_LinhVuc>()
                .HasKey(ts => new { ts.MaSuKien, ts.MaLinhVuc });

            modelBuilder.Entity<TinhNguyenVien_KyNang>()
                .HasKey(tl => new { tl.MaTNV, tl.MaKyNang });
        }
    }

}