using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface ITinhNguyenVienService
    {
        Task<TinhNguyenVienResponseDto> CreateTinhNguyenVienAsync(CreateTinhNguyenVienDto createDto);
        Task<TinhNguyenVienResponseDto> GetTinhNguyenVienAsync(int maTNV);
        Task<List<TinhNguyenVienResponseDto>> GetAllTinhNguyenVienAsync();
        Task<TinhNguyenVienResponseDto> UpdateTinhNguyenVienAsync(int maTNV, UpdateTinhNguyenVienDto updateDto);
        Task<bool> DeleteTinhNguyenVienAsync(int maTNV);
        Task<TinhNguyenVienResponseDto> GetTinhNguyenVienByAccountAsync(int maTaiKhoan);

        Task<string> UploadAnhDaiDienAsync(int maTNV, IFormFile anhFile);
    }
}