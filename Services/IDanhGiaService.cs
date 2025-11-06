using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
   public interface IDanhGiaService
   {
       Task<DanhGiaResponseDto> TaoMoiDanhGiaAsync(CreateDanhGiaDto createDto);
       Task<DanhGiaResponseDto> CapNhatDanhGiaAsync(int maDanhGia, UpdateDanhGiaDto updateDto, int currentUserId, string currentUserRole);
       Task<DanhGiaResponseDto> GetDanhGiaAsync(int maDanhGia);
       Task<List<DanhGiaResponseDto>> GetDanhGiaCuaNguoiAsync(int maNguoi);
       Task<ThongKeDanhGiaDto> GetThongKeDanhGiaAsync(int maNguoi);
       Task<bool> XoaDanhGiaAsync(int maDanhGia, int currentUserId, string currentUserRole);
       
       // Chức năng liên quan đến cấp bậc và điểm uy tín
       Task<List<CapBacDto>> GetDanhSachCapBacAsync();
       Task<string> GetCapBacTheoSoSaoAsync(decimal soSao);
       Task<decimal> TinhDiemTrungBinhAsync(int maNguoi);
       Task<bool> CapNhatDiemVaCapBacAsync(int maNguoi);
   }
}
