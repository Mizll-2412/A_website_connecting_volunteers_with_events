using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface IRecommendationService
    {
        Task<List<EventRecommendationDto>> GetRecommendedEventsAsync(RecommendationRequestDto request);
        Task<decimal> CalculateMatchScoreAsync(int maSuKien, int maTNV, double? locationWeight = 0.3, double? skillWeight = 0.4, double? interestWeight = 0.3);
    }
}
