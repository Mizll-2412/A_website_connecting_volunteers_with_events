using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface IRegistrationFormService
    {
        Task<DonDangKyResponseDto> DangKySuKienAsync(CreateDonDangKyDto createDto);
        Task<DonDangKyResponseDto> GetDonDangKyAsync(int maTNV, int maSuKien);
        Task<List<DonDangKyResponseDto>> GetDonDangKyByTNVAsync(int maTNV);
        Task<List<DonDangKyResponseDto>> GetDonDangKyBySuKienAsync(int maSuKien);
        Task<DonDangKyResponseDto> UpdateTrangThaiAsync(int maTNV, int maSuKien, UpdateDonDangKyDto updateDto);
        Task<bool> HuyDangKyAsync(int maTNV, int maSuKien);
        
        // Lịch sử tham gia sự kiện
        Task<List<EventHistoryDto>> GetEventHistoryAsync(int maTNV, EventHistoryFilterDto filter = null);
        Task<EventHistoryStatsDto> GetEventHistoryStatsAsync(int maTNV);
    }
}
