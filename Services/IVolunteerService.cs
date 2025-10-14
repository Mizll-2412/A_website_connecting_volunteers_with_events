using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface ITinhNguyenVienService
    {
        Task<TinhNguyenVienResponseDto> GetTinhNguyenVienAsync(int maTNV);
        Task<TinhNguyenVienResponseDto> UpdateTinhNguyenVienAsync(int maTNV, UpdateTinhNguyenVienDto updateDto);
        Task<string> UploadAnhDaiDienAsync(int maTNV, IFormFile anhFile);
    }
}