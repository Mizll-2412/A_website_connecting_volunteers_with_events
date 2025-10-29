using khoaluantotnghiep.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace khoaluantotnghiep.Services
{
    public interface ITinhNguyenVienService
    {
        Task<TinhNguyenVienResponseDto> CreateTinhNguyenVienAsync(CreateTNVDto createDto);
        Task<TinhNguyenVienResponseDto> GetTinhNguyenVienAsync(int maTNV);
        Task<List<TinhNguyenVienResponseDto>> GetAllTinhNguyenVienAsync();
        Task<TinhNguyenVienResponseDto> UpdateTinhNguyenVienAsync(int maTNV, UpdateTNVDto updateDto);
        Task<bool> DeleteTinhNguyenVienAsync(int maTNV);
        Task<TinhNguyenVienResponseDto> GetTinhNguyenVienByMaTaiKhoanAsync(int maTaiKhoan);
        Task<List<TinhNguyenVienResponseDto>> GetFeaturedTinhNguyenVienAsync();
        Task<List<KyNangDto>> GetVolunteerSkillsAsync(int maTNV);
        Task<List<LinhVucDto>> GetVolunteerFieldsAsync(int maTNV);
        
        Task<string> UploadAnhDaiDienAsync(int maTNV, IFormFile anhFile);
    }
}