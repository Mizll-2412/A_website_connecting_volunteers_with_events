using khoaluantotnghiep.DTOs;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Services
{
    public interface IStatisticsService
    {
        Task<EventStatisticsDto> GetEventStatisticsAsync(StatisticFilterDto filter = null);
        Task<VolunteerStatisticsDto> GetVolunteerStatisticsAsync(StatisticFilterDto filter = null);
        Task<OrganizationStatisticsDto> GetOrganizationStatisticsAsync(StatisticFilterDto filter = null);
        Task<dynamic> GetDashboardStatisticsAsync(); // Thống kê tổng quan cho dashboard
        Task<dynamic> GetOverallStatisticsAsync(); // Thống kê tổng quan hệ thống
        Task<dynamic> GetRatingStatisticsAsync(); // Thống kê đánh giá
    }
}
