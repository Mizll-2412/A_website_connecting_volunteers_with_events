using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.Models;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public class KyNangService : IKyNangService
    {
        private readonly AppDbContext _context;

        public KyNangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KyNangResponse>> GetAllAsync()
        {
            var kyNangs = await _context.KyNang
                .OrderBy(l => l.TenKyNang)
                .ToListAsync();

            return kyNangs.Select(l => new KyNangResponse
            {
                MaKyNang = l.MaKyNang,
                TenKyNang = l.TenKyNang
            });
        }

        public async Task<KyNangResponse> GetByIdAsync(int id)
        {
            var kyNang = await _context.KyNang.FindAsync(id);
            
            if (kyNang == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            return new KyNangResponse
            {
                MaKyNang = kyNang.MaKyNang,
                TenKyNang = kyNang.TenKyNang
            };
        }

        public async Task<KyNangResponse> CreateAsync(CreateKyNangRequest request)
        {
            // Kiểm tra trùng tên
            var exists = await _context.KyNang
                .AnyAsync(l => l.TenKyNang.ToLower() == request.TenKyNang.ToLower());

            if (exists)
                throw new InvalidOperationException($"Lĩnh vực '{request.TenKyNang}' đã tồn tại");

            var kyNang = new KyNang
            {
                TenKyNang = request.TenKyNang.Trim()
            };

            _context.KyNang.Add(kyNang);
            await _context.SaveChangesAsync();

            return new KyNangResponse
            {
                MaKyNang = kyNang.MaKyNang,
                TenKyNang = kyNang.TenKyNang
            };
        }

        public async Task<KyNangResponse> UpdateAsync(int id, UpdateKyNangRequest request)
        {
            var kyNang = await _context.KyNang.FindAsync(id);
            
            if (kyNang == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            // Kiểm tra trùng tên (trừ chính nó)
            var exists = await _context.KyNang
                .AnyAsync(l => l.TenKyNang.ToLower() == request.TenKyNang.ToLower() 
                            && l.MaKyNang != id);

            if (exists)
                throw new InvalidOperationException($"Lĩnh vực '{request.TenKyNang}' đã tồn tại");

            kyNang.TenKyNang = request.TenKyNang.Trim();
            
            await _context.SaveChangesAsync();

            return new KyNangResponse
            {
                MaKyNang = kyNang.MaKyNang,
                TenKyNang = kyNang.TenKyNang
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var kyNang = await _context.KyNang.FindAsync(id);
            
            if (kyNang == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            // Kiểm tra xem có dữ liệu liên quan không (nếu có foreign key)
            // Ví dụ: Kiểm tra xem có hoạt động nào thuộc lĩnh vực này không
            // var hasRelatedData = await _context.HoatDong.AnyAsync(h => h.MaKyNang == id);
            // if (hasRelatedData)
            //     throw new InvalidOperationException("Không thể xóa lĩnh vực đang được sử dụng");

            _context.KyNang.Remove(kyNang);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}