using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface IOrganizationService
    {
        Task<ToChucResponseDto> GetToChucAsync(int maToChuc);
        Task<ToChucResponseDto> UpdateToChucAsync(int maToChuc, UpdateToChucDto updateDto);
        Task<string> UploadAnhDaiDienAsync(int maToChuc, IFormFile anhFile);
    }
}
