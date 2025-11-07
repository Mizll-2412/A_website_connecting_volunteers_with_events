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
        public DbSet<TokenResetMatKhau> TokenResetMatKhau { get; set; }
        public DbSet<TokenDoiEmail> TokenDoiEmail { get; set; }
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
                
            // Cấu hình NguoiNhanThongBao với composite key (MaNguoiNhanThongBao, MaThongBao)
            modelBuilder.Entity<NguoiNhanThongBao>()
                .HasKey(n => new { n.MaNguoiNhanThongBao, n.MaThongBao });
                
            // Cấu hình NguoiNhanThongBao để tránh multiple cascade paths
            modelBuilder.Entity<NguoiNhanThongBao>()
                .HasOne(n => n.ThongBao)
                .WithMany()
                .HasForeignKey(n => n.MaThongBao)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Cấu hình DanhGia để tránh multiple cascade paths
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.NguoiDanhGia)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiDanhGia)
                .OnDelete(DeleteBehavior.NoAction);
                
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.NguoiDuocDanhGia)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiDuocDanhGia)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Cấu hình DonDangKy để tránh multiple cascade paths
            modelBuilder.Entity<DonDangKy>()
                .HasKey(d => new { d.MaTNV, d.MaSuKien });
                
            modelBuilder.Entity<DonDangKy>()
                .HasOne(d => d.SuKien)
                .WithMany()
                .HasForeignKey(d => d.MaSuKien)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Cấu hình GiayChungNhan để tránh multiple cascade paths
            modelBuilder.Entity<GiayChungNhan>()
                .HasOne(g => g.TinhNguyenVien)
                .WithMany()
                .HasForeignKey(g => g.MaTNV)
                .OnDelete(DeleteBehavior.NoAction);
                
            // Cấu hình precision cho decimal properties
            modelBuilder.Entity<TinhNguyenVien>()
                .Property(t => t.DiemTrungBinh)
                .HasPrecision(3, 1);
                
            modelBuilder.Entity<ToChuc>()
                .Property(t => t.DiemTrungBinh)
                .HasPrecision(3, 1);
        }
    }

}