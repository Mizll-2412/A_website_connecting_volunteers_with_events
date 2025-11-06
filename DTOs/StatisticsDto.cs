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
        public double AverageVolunteersPerEvent { get; set; }
        public double AverageRating { get; set; }
    }

    public class VolunteerStatisticsDto
    {
        public int TotalVolunteers { get; set; }
        public int ActiveVolunteers { get; set; }
        public Dictionary<string, int> VolunteersByRank { get; set; } = new Dictionary<string, int>();
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
