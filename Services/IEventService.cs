using khoaluantotnghiep.DTOs;
namespace khoaluantotnghiep.Services
{
    public interface IEventService
    {
        Task<SuKienResponseDto> CreateSuKienAsync(CreateSuKienDto createDto);
        Task<SuKienResponseDto> GetSuKienAsync(int maSuKien);
        Task<List<SuKienResponseDto>> GetAllSuKienAsync();
        Task<SuKienResponseDto> UpdateSuKienAsync(int maSuKien, UpdateSuKienDto updateDto);
        Task<bool> DeleteSuKienAsync(int maSuKien);
        Task<string> UploadAnhAsync(int maToChuc, IFormFile anhFile);
        Task<string> UploadAnh(IFormFile anhFile);


    }
}