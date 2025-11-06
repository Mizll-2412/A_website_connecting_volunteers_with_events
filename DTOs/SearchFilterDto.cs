using System;
using System.Collections.Generic;

namespace khoaluantotnghiep.DTOs
{
    public class EventSearchFilterDto
    {
        // Tìm kiếm cơ bản
        public string Keyword { get; set; }

        // Lọc theo thời gian
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Lọc theo địa điểm
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? RadiusKm { get; set; } // Bán kính tìm kiếm theo km

        // Lọc theo lĩnh vực và kỹ năng
        public List<int>? FieldIds { get; set; }
        public List<int>? SkillIds { get; set; }

        // Lọc theo tổ chức
        public List<int>? OrganizationIds { get; set; }
        public bool? OnlyVerifiedOrganizations { get; set; }

        // Lọc theo trạng thái sự kiện
        public List<int>? StatusIds { get; set; } // 0: Chưa diễn ra, 1: Đang diễn ra, 2: Đã kết thúc, 3: Đã hủy

        // Lọc theo số lượng tình nguyện viên
        public int? MinVolunteers { get; set; }
        public int? MaxVolunteers { get; set; }

        // Sắp xếp
        public string SortBy { get; set; } // date_asc, date_desc, popularity, distance
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class VolunteerSearchFilterDto
    {
        // Tìm kiếm cơ bản
        public string Keyword { get; set; }

        // Lọc theo thông tin cá nhân
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string Gender { get; set; }
        public string Location { get; set; }

        // Lọc theo lĩnh vực và kỹ năng
        public List<int>? FieldIds { get; set; }
        public List<int>? SkillIds { get; set; }

        // Lọc theo điểm uy tín và cấp bậc
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }
        public List<string>? RankNames { get; set; }

        // Lọc theo số sự kiện tham gia
        public int? MinEvents { get; set; }

        // Sắp xếp
        public string SortBy { get; set; } // rating_desc, events_desc, name_asc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class OrganizationSearchFilterDto
    {
        // Tìm kiếm cơ bản
        public string Keyword { get; set; }

        // Lọc theo địa điểm
        public string Location { get; set; }

        // Lọc theo lĩnh vực
        public List<int>? FieldIds { get; set; }

        // Lọc theo trạng thái xác minh
        public int? VerificationStatus { get; set; } // 0: Chưa xác minh, 1: Đã xác minh, 2: Đang chờ xác minh

        // Lọc theo điểm uy tín
        public decimal? MinRating { get; set; }
        public decimal? MaxRating { get; set; }

        // Lọc theo số sự kiện đã tổ chức
        public int? MinEvents { get; set; }

        // Sắp xếp
        public string SortBy { get; set; } // rating_desc, events_desc, name_asc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SearchResultPaginationDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public Dictionary<string, dynamic> Facets { get; set; } // Thông tin thống kê cho bộ lọc
    }
}
