using khoaluantotnghiep.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public interface IOrganizationService
    {
        Task<ToChucResponseDto> GetToChucAsync(int maToChuc);
        Task<ToChucResponseDto> GetToChucByMaTaiKhoanAsync(int maTaiKhoan);
        Task<ToChucResponseDto> UpdateToChucAsync(int maToChuc, UpdateToChucDto updateDto);
        Task<ToChucResponseDto> CreateToChucAsync(CreateToChucDto createDto);
        Task<List<ToChucResponseDto>> GetAllToChucAsync();
        Task<bool> DeleteToChucAsync(int maTNV);
        Task<string> UploadAnhDaiDienAsync(int maToChuc, IFormFile anhFile);
        
        // Chức năng liên quan đến xác minh tổ chức
        Task<VerificationStatusResponseDto> RequestVerificationAsync(RequestVerificationDto requestDto);
        Task<VerificationStatusResponseDto> GetVerificationStatusAsync(int maToChuc);
    }
}
