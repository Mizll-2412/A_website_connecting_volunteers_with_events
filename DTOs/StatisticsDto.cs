using System;
using System.Collections.Generic;

namespace khoaluantotnghiep.DTOs
{
    public class EventStatisticsDto
    {
        public int TotalEvents { get; set; }
        public int PendingEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int CancelledEvents { get; set; }
        public Dictionary<string, int> EventsByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> EventsByField { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, EventMonthlyBreakdownDto> EventsByMonthDetailed { get; set; } = new Dictionary<string, EventMonthlyBreakdownDto>();
        public double AverageVolunteersPerEvent { get; set; }
        public double AverageRating { get; set; }
    }

    public class EventMonthlyBreakdownDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class VolunteerStatisticsDto
    {
        public int TotalVolunteers { get; set; }
        public int ActiveVolunteers { get; set; }
        public Dictionary<string, int> VolunteersByRank { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VolunteersByGender { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VolunteersByField { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VolunteersByAge { get; set; } = new Dictionary<string, int>();
        public double AverageRating { get; set; }
        public double AverageEventsPerVolunteer { get; set; }
    }

    public class OrganizationStatisticsDto
    {
        public int TotalOrganizations { get; set; }
        public int VerifiedOrganizations { get; set; }
        public int PendingVerificationOrganizations { get; set; }
        public Dictionary<string, int> OrganizationsByField { get; set; } = new Dictionary<string, int>();
        public double AverageRating { get; set; }
        public double AverageEventsPerOrganization { get; set; }
    }

    public class OrganizationSpecificStatisticsDto
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        
        // Thống kê sự kiện
        public int TotalEvents { get; set; }
        public int PendingEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int CancelledEvents { get; set; }
        public Dictionary<string, int> EventsByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> EventsByField { get; set; } = new Dictionary<string, int>();
        public double AverageVolunteersPerEvent { get; set; }
        public double AverageEventRating { get; set; }
        
        // Thống kê đăng ký
        public int TotalRegistrations { get; set; }
        public int PendingRegistrations { get; set; }
        public int ApprovedRegistrations { get; set; }
        public int RejectedRegistrations { get; set; }
        public Dictionary<string, int> RegistrationsByMonth { get; set; } = new Dictionary<string, int>();
        public double ApprovalRate { get; set; }
        
        // Thống kê tình nguyện viên
        public int TotalVolunteers { get; set; }
        public int NewVolunteersThisMonth { get; set; }
        public Dictionary<string, int> VolunteersByRank { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> VolunteersByGender { get; set; } = new Dictionary<string, int>();
        public double AverageVolunteerRating { get; set; }
        
        // Thống kê đánh giá
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingsDistribution { get; set; } = new Dictionary<int, int>();
        public int TotalRatings { get; set; }
    }

    public class StatisticFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public List<int>? FieldIds { get; set; }
        public List<int>? OrganizationIds { get; set; }
    }
}
