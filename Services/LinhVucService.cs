using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.Models;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public class LinhVucService : ILinhVucService
    {
        private readonly AppDbContext _context;

        public LinhVucService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LinhVucResponse>> GetAllAsync()
        {
            var linhVucs = await _context.LinhVuc
                .OrderBy(l => l.TenLinhVuc)
                .ToListAsync();

            return linhVucs.Select(l => new LinhVucResponse
            {
                MaLinhVuc = l.MaLinhVuc,
                TenLinhVuc = l.TenLinhVuc
            });
        }

        public async Task<LinhVucResponse> GetByIdAsync(int id)
        {
            var linhVuc = await _context.LinhVuc.FindAsync(id);
            
            if (linhVuc == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            return new LinhVucResponse
            {
                MaLinhVuc = linhVuc.MaLinhVuc,
                TenLinhVuc = linhVuc.TenLinhVuc
            };
        }

        public async Task<LinhVucResponse> CreateAsync(CreateLinhVucRequest request)
        {
            // Kiểm tra trùng tên
            var exists = await _context.LinhVuc
                .AnyAsync(l => l.TenLinhVuc.ToLower() == request.TenLinhVuc.ToLower());

            if (exists)
                throw new InvalidOperationException($"Lĩnh vực '{request.TenLinhVuc}' đã tồn tại");

            var linhVuc = new LinhVuc
            {
                TenLinhVuc = request.TenLinhVuc.Trim()
            };

            _context.LinhVuc.Add(linhVuc);
            await _context.SaveChangesAsync();

            return new LinhVucResponse
            {
                MaLinhVuc = linhVuc.MaLinhVuc,
                TenLinhVuc = linhVuc.TenLinhVuc
            };
        }

        public async Task<LinhVucResponse> UpdateAsync(int id, UpdateLinhVucRequest request)
        {
            var linhVuc = await _context.LinhVuc.FindAsync(id);
            
            if (linhVuc == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            // Kiểm tra trùng tên (trừ chính nó)
            var exists = await _context.LinhVuc
                .AnyAsync(l => l.TenLinhVuc.ToLower() == request.TenLinhVuc.ToLower() 
                            && l.MaLinhVuc != id);

            if (exists)
                throw new InvalidOperationException($"Lĩnh vực '{request.TenLinhVuc}' đã tồn tại");

            linhVuc.TenLinhVuc = request.TenLinhVuc.Trim();
            
            await _context.SaveChangesAsync();

            return new LinhVucResponse
            {
                MaLinhVuc = linhVuc.MaLinhVuc,
                TenLinhVuc = linhVuc.TenLinhVuc
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var linhVuc = await _context.LinhVuc.FindAsync(id);
            
            if (linhVuc == null)
                throw new KeyNotFoundException($"Không tìm thấy lĩnh vực với mã {id}");

            // Kiểm tra xem có dữ liệu liên quan không (nếu có foreign key)
            // Ví dụ: Kiểm tra xem có hoạt động nào thuộc lĩnh vực này không
            // var hasRelatedData = await _context.HoatDong.AnyAsync(h => h.MaLinhVuc == id);
            // if (hasRelatedData)
            //     throw new InvalidOperationException("Không thể xóa lĩnh vực đang được sử dụng");

            _context.LinhVuc.Remove(linhVuc);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}