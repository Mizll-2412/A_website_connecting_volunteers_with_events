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
}
}
