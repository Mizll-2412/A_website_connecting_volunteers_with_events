using System.Threading.Tasks;
using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface ISearchService
    {
        // Tìm kiếm nâng cao cho sự kiện
        Task<SearchResultPaginationDto<SuKienResponseDto>> SearchEventsAsync(EventSearchFilterDto filter);
        
        // Tìm kiếm nâng cao cho tình nguyện viên
        Task<SearchResultPaginationDto<TinhNguyenVienResponseDto>> SearchVolunteersAsync(VolunteerSearchFilterDto filter);
        
        // Tìm kiếm nâng cao cho tổ chức
        Task<SearchResultPaginationDto<ToChucResponseDto>> SearchOrganizationsAsync(OrganizationSearchFilterDto filter);
        
        // Tìm kiếm tất cả (đa dạng kết quả)
        Task<dynamic> SearchAllAsync(string keyword, int page = 1, int pageSize = 10);
    }
}
