using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;
using Microsoft.Extensions.Logging;

namespace khoaluantotnghiep.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(AppDbContext context, ILogger<StatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EventStatisticsDto> GetEventStatisticsAsync(StatisticFilterDto? filter = null)
        {
            try
            {
                // Khởi tạo query cơ bản
                var query = _context.Event.AsQueryable();

                // Áp dụng filter nếu có
                if (filter != null)
                {
                    if (filter.FromDate.HasValue)
                        query = query.Where(e => e.NgayBatDau >= filter.FromDate);

                    if (filter.ToDate.HasValue)
                        query = query.Where(e => e.NgayBatDau <= filter.ToDate);

                    if (filter.Year.HasValue)
                        query = query.Where(e => e.NgayBatDau.HasValue && e.NgayBatDau.Value.Year == filter.Year);

                    if (filter.Month.HasValue && filter.Year.HasValue)
                        query = query.Where(e => e.NgayBatDau.HasValue && e.NgayBatDau.Value.Month == filter.Month && e.NgayBatDau.Value.Year == filter.Year);

                    if (filter.OrganizationIds != null && filter.OrganizationIds.Any())
                        query = query.Where(e => filter.OrganizationIds.Contains(e.MaToChuc));

                    if (filter.FieldIds != null && filter.FieldIds.Any())
                    {
                        query = query.Where(e => e.SuKien_LinhVucs.Any(sl => filter.FieldIds.Contains(sl.MaLinhVuc)));
                    }
                }

                // Lấy danh sách sự kiện sau khi filter
                var events = await query.Include(e => e.SuKien_LinhVucs)
                                        .ThenInclude(sl => sl.LinhVuc)
                                        .ToListAsync();

                // Tính toán thống kê
                var now = DateTime.Now;
                var pending = events.Count(e => e.NgayBatDau > now); // Sắp diễn ra
                var active = events.Count(e => e.NgayBatDau <= now && (!e.NgayKetThuc.HasValue || e.NgayKetThuc >= now)); // Đang diễn ra
                var completed = events.Count(e => e.NgayKetThuc.HasValue && e.NgayKetThuc < now); // Đã kết thúc
                var cancelled = events.Count(e => e.TrangThai == "2"); // Đã hủy (TrangThai = "2")

                // Thống kê theo tháng
                var eventsWithStartDate = events.Where(e => e.NgayBatDau.HasValue).ToList();

                var eventsByMonth = eventsWithStartDate
                    .GroupBy(e => new
                    {
                        Year = e.NgayBatDau!.Value.Year,
                        Month = e.NgayBatDau!.Value.Month
                    })
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:00}",
                        g => g.Count()
                    );

                var eventsByMonthDetailed = eventsWithStartDate
                    .GroupBy(e => new
                    {
                        Year = e.NgayBatDau!.Value.Year,
                        Month = e.NgayBatDau!.Value.Month
                    })
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:00}",
                        g => new EventMonthlyBreakdownDto
                        {
                            Total = g.Count(),
                            Pending = g.Count(e => e.NgayBatDau > now),
                            Active = g.Count(e => e.NgayBatDau <= now && (!e.NgayKetThuc.HasValue || e.NgayKetThuc >= now)),
                            Completed = g.Count(e => e.NgayKetThuc.HasValue && e.NgayKetThuc < now),
                            Cancelled = g.Count(e => e.TrangThai == "2")
                        }
                    );

                // Thống kê theo lĩnh vực
                var eventsByField = new Dictionary<string, int>();
                foreach (var evt in events)
                {
                    foreach (var field in evt.SuKien_LinhVucs.Select(sl => sl.LinhVuc))
                    {
                        if (field != null)
                        {
                            string fieldName = field.TenLinhVuc ?? "Khác";
                            if (eventsByField.ContainsKey(fieldName))
                                eventsByField[fieldName]++;
                            else
                                eventsByField[fieldName] = 1;
                        }
                    }
                }

                // Tính số tình nguyện viên trung bình mỗi sự kiện
                double avgVolunteers = 0;
                if (events.Any())
                {
                    var eventIds = events.Select(e => e.MaSuKien).ToList();
                    var registrations = await _context.DonDangKy
                                                .Where(d => eventIds.Contains(d.MaSuKien) && d.TrangThai == 1) // Chỉ đếm đơn đã duyệt
                                                .GroupBy(d => d.MaSuKien)
                                                .Select(g => new { EventId = g.Key, Count = g.Count() })
                                                .ToListAsync();

                    avgVolunteers = registrations.Any() ? registrations.Average(r => r.Count) : 0;
                }

                // Tính điểm đánh giá trung bình
                double avgRating = 0;
                if (events.Any())
                {
                    var eventIds = events.Select(e => e.MaSuKien).ToList();
                    var ratings = await _context.DanhGia
                                          .Where(d => eventIds.Contains(d.MaSuKien))
                                          .ToListAsync();

                    avgRating = ratings.Any() ? ratings.Average(r => r.DiemSo) : 0;
                }

                return new EventStatisticsDto
                {
                    TotalEvents = events.Count,
                    PendingEvents = pending,
                    ActiveEvents = active,
                    CompletedEvents = completed,
                    CancelledEvents = cancelled,
                    EventsByMonth = eventsByMonth,
                    EventsByField = eventsByField,
                    EventsByMonthDetailed = eventsByMonthDetailed,
                    AverageVolunteersPerEvent = Math.Round(avgVolunteers, 2),
                    AverageRating = Math.Round(avgRating, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<VolunteerStatisticsDto> GetVolunteerStatisticsAsync(StatisticFilterDto? filter = null)
        {
            try
            {
                // Khởi tạo query cơ bản
                var query = _context.Volunteer.AsQueryable();

                // Lấy danh sách tình nguyện viên
                var volunteers = await query.Include(v => v.TinhNguyenVien_LinhVucs)
                                           .ThenInclude(vl => vl.LinhVuc)
                                           .ToListAsync();

                // Tính tổng số tình nguyện viên
                var totalVolunteers = volunteers.Count;

                // Tính số tình nguyện viên hoạt động (tham gia ít nhất 1 sự kiện)
                var volunteerIds = volunteers.Select(v => v.MaTNV).ToList();
                var activeVolunteerIds = await _context.DonDangKy
                                                    .Where(d => volunteerIds.Contains(d.MaTNV) && d.TrangThai == 1) // Đã duyệt
                                                    .Select(d => d.MaTNV)
                                                    .Distinct()
                                                    .ToListAsync();
                var activeVolunteers = activeVolunteerIds.Count;

                // Thống kê theo cấp bậc
                var volunteersByRank = volunteers.GroupBy(v => v.CapBac ?? "Chưa xác định")
                                                .ToDictionary(
                                                    g => g.Key,
                                                    g => g.Count()
                                                );

                // Thống kê theo giới tính
                var volunteersByGender = volunteers
                    .Select(v => NormalizeGender(v.GioiTinh))
                    .GroupBy(g => g)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Ensure full gender keys are present
                foreach (var key in new[] { "Nam", "Nữ", "Khác" })
                {
                    if (!volunteersByGender.ContainsKey(key))
                    {
                        volunteersByGender[key] = 0;
                    }
                }

                // Thống kê theo lĩnh vực quan tâm
                var volunteersByField = new Dictionary<string, int>();
                foreach (var volunteer in volunteers)
                {
                    foreach (var field in volunteer.TinhNguyenVien_LinhVucs.Select(vl => vl.LinhVuc))
                    {
                        if (field != null)
                        {
                            string fieldName = field.TenLinhVuc ?? "Khác";
                            if (volunteersByField.ContainsKey(fieldName))
                                volunteersByField[fieldName]++;
                            else
                                volunteersByField[fieldName] = 1;
                        }
                    }
                }

                // Thống kê theo độ tuổi
                var volunteersByAge = new Dictionary<string, int>
                {
                    { "18-24", 0 },
                    { "25-34", 0 },
                    { "35-44", 0 },
                    { "45-54", 0 },
                    { "55+", 0 },
                    { "Không xác định", 0 }
                };

                foreach (var volunteer in volunteers)
                {
                    if (!volunteer.NgaySinh.HasValue)
                    {
                        volunteersByAge["Không xác định"]++;
                        continue;
                    }

                    var age = DateTime.Today.Year - volunteer.NgaySinh.Value.Year;
                    // Kiểm tra nếu chưa tới sinh nhật năm nay
                    if (volunteer.NgaySinh.Value.Date > DateTime.Today.AddYears(-age)) age--;

                    if (age >= 18 && age <= 24)
                        volunteersByAge["18-24"]++;
                    else if (age >= 25 && age <= 34)
                        volunteersByAge["25-34"]++;
                    else if (age >= 35 && age <= 44)
                        volunteersByAge["35-44"]++;
                    else if (age >= 45 && age <= 54)
                        volunteersByAge["45-54"]++;
                    else if (age >= 55)
                        volunteersByAge["55+"]++;
                    else
                        volunteersByAge["Không xác định"]++;
                }

                // Tính điểm đánh giá trung bình
                double avgRating = (double)volunteers.Where(v => v.DiemTrungBinh.HasValue)
                                           .Select(v => v.DiemTrungBinh!.Value)
                                           .DefaultIfEmpty(0)
                                           .Average();

                // Tính số sự kiện trung bình mỗi tình nguyện viên
                double avgEvents = 0;
                if (volunteers.Any())
                {
                    var eventCounts = await _context.DonDangKy
                                               .Where(d => volunteerIds.Contains(d.MaTNV) && d.TrangThai == 1) // Chỉ đếm đơn đã duyệt
                                               .GroupBy(d => d.MaTNV)
                                               .Select(g => new { VolunteerId = g.Key, Count = g.Count() })
                                               .ToListAsync();

                    avgEvents = eventCounts.Any() ? eventCounts.Average(r => r.Count) : 0;
                }

                return new VolunteerStatisticsDto
                {
                    TotalVolunteers = totalVolunteers,
                    ActiveVolunteers = activeVolunteers,
                    VolunteersByRank = volunteersByRank,
                    VolunteersByGender = volunteersByGender,
                    VolunteersByField = volunteersByField,
                    VolunteersByAge = volunteersByAge,
                    AverageRating = Math.Round(avgRating, 2),
                    AverageEventsPerVolunteer = Math.Round(avgEvents, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        private static string NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
            {
                return "Khác";
            }

            var normalized = gender.Trim().ToLower();
            return normalized switch
            {
                "nam" or "male" => "Nam",
                "nữ" or "nu" or "female" => "Nữ",
                _ => "Khác"
            };
        }

        public async Task<OrganizationStatisticsDto> GetOrganizationStatisticsAsync(StatisticFilterDto? filter = null)
        {
            try
            {
                // Khởi tạo query cơ bản
                var query = _context.Organization.AsQueryable();

                // Lấy danh sách tổ chức
                var organizations = await query.ToListAsync();

                // Tính tổng số tổ chức
                var totalOrganizations = organizations.Count;
                var verifiedOrganizations = organizations.Count(o => o.TrangThaiXacMinh == 1); // Đã xác minh
                var pendingVerificationOrganizations = organizations.Count(o => o.TrangThaiXacMinh == 2); // Đang chờ xác minh

                // Thống kê theo lĩnh vực
                var organizationIds = organizations.Select(o => o.MaToChuc).ToList();
                var eventsByOrg = await _context.Event
                                           .Where(e => organizationIds.Contains(e.MaToChuc))
                                           .Include(e => e.SuKien_LinhVucs)
                                           .ThenInclude(sl => sl.LinhVuc)
                                           .ToListAsync();

                var organizationsByField = new Dictionary<string, int>();
                foreach (var org in organizations)
                {
                    var orgEvents = eventsByOrg.Where(e => e.MaToChuc == org.MaToChuc).ToList();
                    var orgFields = new HashSet<string>();

                    foreach (var evt in orgEvents)
                    {
                        foreach (var field in evt.SuKien_LinhVucs.Select(sl => sl.LinhVuc?.TenLinhVuc).Where(f => !string.IsNullOrWhiteSpace(f)))
                        {
                            orgFields.Add(field!);
                        }
                    }

                    foreach (var field in orgFields)
                    {
                        if (organizationsByField.ContainsKey(field))
                            organizationsByField[field]++;
                        else
                            organizationsByField[field] = 1;
                    }
                }

                // Tính điểm đánh giá trung bình
                double avgRating = (double)organizations.Where(o => o.DiemTrungBinh.HasValue)
                                             .Select(o => o.DiemTrungBinh!.Value)
                                             .DefaultIfEmpty(0)
                                             .Average();

                // Tính số sự kiện trung bình mỗi tổ chức
                var eventCounts = eventsByOrg.GroupBy(e => e.MaToChuc)
                                          .ToDictionary(
                                              g => g.Key,
                                              g => g.Count()
                                          );

                double avgEvents = eventCounts.Any() ? eventCounts.Values.Average() : 0;

                return new OrganizationStatisticsDto
                {
                    TotalOrganizations = totalOrganizations,
                    VerifiedOrganizations = verifiedOrganizations,
                    PendingVerificationOrganizations = pendingVerificationOrganizations,
                    OrganizationsByField = organizationsByField,
                    AverageRating = Math.Round(avgRating, 2),
                    AverageEventsPerOrganization = Math.Round(avgEvents, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê tổ chức: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetDashboardStatisticsAsync()
        {
            try
            {
                // Tổng số tình nguyện viên
                var totalVolunteers = await _context.Volunteer.CountAsync();

                // Tổng số tổ chức
                var totalOrganizations = await _context.Organization.CountAsync();

                // Tổng số sự kiện
                var totalEvents = await _context.Event.CountAsync();
                
                // Sự kiện đang diễn ra
                var now = DateTime.Now;
                var activeEvents = await _context.Event
                                          .CountAsync(e => e.NgayBatDau <= now && 
                                                     (!e.NgayKetThuc.HasValue || e.NgayKetThuc >= now));
                
                // Sự kiện sắp tới (trong 7 ngày tới)
                var upcomingEvents = await _context.Event
                                           .CountAsync(e => e.NgayBatDau > now && 
                                                      e.NgayBatDau <= now.AddDays(7));
                
                // Tổng số đơn đăng ký
                var totalRegistrations = await _context.DonDangKy.CountAsync();
                
                // Đơn đang chờ duyệt
                var pendingRegistrations = await _context.DonDangKy
                                                .CountAsync(d => d.TrangThai == 0); // 0: Chờ duyệt
                
                // Số đánh giá gần đây (30 ngày qua)
                var recentRatings = await _context.DanhGia
                                          .CountAsync(d => d.NgayTao >= now.AddDays(-30));
                
                // 10 tình nguyện viên có điểm cao nhất
                var topVolunteers = await _context.Volunteer
                                          .Where(v => v.DiemTrungBinh.HasValue)
                                          .OrderByDescending(v => v.DiemTrungBinh)
                                          .Take(10)
                                          .Select(v => new
                                          {
                                              MaTNV = v.MaTNV,
                                              HoTen = v.HoTen,
                                              DiemTrungBinh = v.DiemTrungBinh,
                                              CapBac = v.CapBac,
                                              TongSuKienThamGia = v.TongSuKienThamGia,
                                              AnhDaiDien = v.AnhDaiDien
                                          })
                                          .ToListAsync();
                
                // 10 tổ chức có điểm cao nhất
                var topOrganizations = await _context.Organization
                                             .Where(o => o.DiemTrungBinh.HasValue)
                                             .OrderByDescending(o => o.DiemTrungBinh)
                                             .Take(10)
                                             .Select(o => new
                                             {
                                                 MaToChuc = o.MaToChuc,
                                                 TenToChuc = o.TenToChuc,
                                                 DiemTrungBinh = o.DiemTrungBinh,
                                                 Logo = o.AnhDaiDien,
                                                 TrangThaiXacMinh = o.TrangThaiXacMinh
                                             })
                                             .ToListAsync();

                // Thống kê sự kiện theo tháng trong năm hiện tại
                var currentYear = now.Year;
                var eventsByMonth = await _context.Event
                                          .Where(e => e.NgayBatDau.HasValue && e.NgayBatDau!.Value.Year == currentYear)
                                          .GroupBy(e => e.NgayBatDau!.Value.Month)
                                          .Select(g => new { Month = g.Key, Count = g.Count() })
                                          .ToDictionaryAsync(
                                              g => g.Month.ToString(), 
                                              g => g.Count
                                          );
                
                // Đảm bảo đủ 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    var monthKey = i.ToString();
                    if (!eventsByMonth.ContainsKey(monthKey))
                        eventsByMonth[monthKey] = 0;
                }

                // Thống kê đăng ký theo tháng trong năm hiện tại
                var registrationsByMonth = await _context.DonDangKy
                                                 .Where(d => d.NgayTao.Year == currentYear)
                                                 .GroupBy(d => d.NgayTao.Month)
                                                 .Select(g => new { Month = g.Key, Count = g.Count() })
                                                 .ToDictionaryAsync(
                                                     g => g.Month.ToString(),
                                                     g => g.Count
                                                 );

                // Đảm bảo đủ 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    var monthKey = i.ToString();
                    if (!registrationsByMonth.ContainsKey(monthKey))
                        registrationsByMonth[monthKey] = 0;
                }

                return new
                {
                    TotalVolunteers = totalVolunteers,
                    TotalOrganizations = totalOrganizations,
                    TotalEvents = totalEvents,
                    ActiveEvents = activeEvents,
                    UpcomingEvents = upcomingEvents,
                    TotalRegistrations = totalRegistrations,
                    PendingRegistrations = pendingRegistrations,
                    RecentRatings = recentRatings,
                    TopVolunteers = topVolunteers,
                    TopOrganizations = topOrganizations,
                    EventsByMonth = eventsByMonth,
                    RegistrationsByMonth = registrationsByMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê tổng quan: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetOverallStatisticsAsync()
        {
            try
            {
                // Tổng số người dùng (tất cả roles)
                var totalUsers = await _context.User.CountAsync();

                // Tổng số sự kiện
                var totalEvents = await _context.Event.CountAsync();

                // Tổng số tình nguyện viên
                var totalVolunteers = await _context.Volunteer.CountAsync();

                // Tổng số tổ chức
                var totalOrganizations = await _context.Organization.CountAsync();

                // Tổng số đơn đăng ký
                var totalRegistrations = await _context.DonDangKy.CountAsync();

                // Đơn đăng ký theo trạng thái
                var registrationsByStatus = new Dictionary<string, int>
                {
                    { "pending", await _context.DonDangKy.CountAsync(d => d.TrangThai == 0) },
                    { "approved", await _context.DonDangKy.CountAsync(d => d.TrangThai == 1) },
                    { "rejected", await _context.DonDangKy.CountAsync(d => d.TrangThai == 2) }
                };

                return new
                {
                    totalUsers = totalUsers,
                    totalEvents = totalEvents,
                    totalVolunteers = totalVolunteers,
                    totalOrganizations = totalOrganizations,
                    totalRegistrations = totalRegistrations,
                    registrationsByStatus = registrationsByStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê tổng quan: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> GetRatingStatisticsAsync()
        {
            try
            {
                // Điểm đánh giá trung bình của tình nguyện viên
                var volunteerRatings = await _context.Volunteer
                    .Where(v => v.DiemTrungBinh.HasValue)
                    .Select(v => v.DiemTrungBinh!.Value)
                    .ToListAsync();
                var averageVolunteerRating = volunteerRatings.Any() ? volunteerRatings.Average() : 0;

                // Điểm đánh giá trung bình của tổ chức
                var organizationRatings = await _context.Organization
                    .Where(o => o.DiemTrungBinh.HasValue)
                    .Select(o => o.DiemTrungBinh!.Value)
                    .ToListAsync();
                var averageOrganizationRating = organizationRatings.Any() ? organizationRatings.Average() : 0;

                // Phân bố điểm đánh giá tình nguyện viên (1-5)
                var volunteerRatingsDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    volunteerRatingsDistribution[i] = volunteerRatings.Count(r => Math.Round(r) == i);
                }

                // Phân bố điểm đánh giá tổ chức (1-5)
                var organizationRatingsDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    organizationRatingsDistribution[i] = organizationRatings.Count(r => Math.Round(r) == i);
                }

                return new
                {
                    averageVolunteerRating = Math.Round(averageVolunteerRating, 2),
                    averageOrganizationRating = Math.Round(averageOrganizationRating, 2),
                    volunteerRatingsDistribution = volunteerRatingsDistribution,
                    organizationRatingsDistribution = organizationRatingsDistribution
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê đánh giá: {ex.Message}");
                throw;
            }
        }

        public async Task<OrganizationSpecificStatisticsDto> GetOrganizationSpecificStatisticsAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organization
                    .FirstOrDefaultAsync(o => o.MaToChuc == organizationId);

                if (organization == null)
                {
                    throw new Exception($"Không tìm thấy tổ chức với ID: {organizationId}");
                }

                var now = DateTime.Now;
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);

                // Lấy tất cả sự kiện của tổ chức
                var events = await _context.Event
                    .Where(e => e.MaToChuc == organizationId)
                    .Include(e => e.SuKien_LinhVucs)
                    .ThenInclude(sl => sl.LinhVuc)
                    .ToListAsync();

                // Thống kê sự kiện
                var pendingEvents = events.Count(e => e.NgayBatDau > now);
                var activeEvents = events.Count(e => e.NgayBatDau <= now && (!e.NgayKetThuc.HasValue || e.NgayKetThuc >= now));
                var completedEvents = events.Count(e => e.NgayKetThuc.HasValue && e.NgayKetThuc < now);
                var cancelledEvents = events.Count(e => e.TrangThai == "2");

                // Sự kiện theo tháng
                var eventsByMonth = events
                    .Where(e => e.NgayBatDau.HasValue)
                    .GroupBy(e => new { Year = e.NgayBatDau!.Value.Year, Month = e.NgayBatDau!.Value.Month })
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:00}",
                        g => g.Count()
                    );

                // Sự kiện theo lĩnh vực
                var eventsByField = new Dictionary<string, int>();
                foreach (var evt in events)
                {
                    foreach (var field in evt.SuKien_LinhVucs.Select(sl => sl.LinhVuc))
                    {
                        if (field != null)
                        {
                            string fieldName = field.TenLinhVuc ?? "Khác";
                            if (eventsByField.ContainsKey(fieldName))
                                eventsByField[fieldName]++;
                            else
                                eventsByField[fieldName] = 1;
                        }
                    }
                }

                // Lấy tất cả đơn đăng ký của các sự kiện thuộc tổ chức
                var eventIds = events.Select(e => e.MaSuKien).ToList();
                var registrations = await _context.DonDangKy
                    .Where(d => eventIds.Contains(d.MaSuKien))
                    .ToListAsync();

                // Thống kê đăng ký
                var pendingRegistrations = registrations.Count(d => d.TrangThai == 0);
                var approvedRegistrations = registrations.Count(d => d.TrangThai == 1);
                var rejectedRegistrations = registrations.Count(d => d.TrangThai == 2);
                var approvalRate = registrations.Any() 
                    ? (double)approvedRegistrations / registrations.Count * 100 
                    : 0;

                // Đăng ký theo tháng
                var registrationsByMonth = registrations
                    .Where(d => d.NgayTao.Year == currentYear)
                    .GroupBy(d => d.NgayTao.Month)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => g.Count()
                    );

                // Đảm bảo đủ 12 tháng
                for (int i = 1; i <= 12; i++)
                {
                    var monthKey = i.ToString();
                    if (!registrationsByMonth.ContainsKey(monthKey))
                        registrationsByMonth[monthKey] = 0;
                }

                // Lấy danh sách tình nguyện viên đã tham gia (đã duyệt đơn)
                var volunteerIds = registrations
                    .Where(d => d.TrangThai == 1)
                    .Select(d => d.MaTNV)
                    .Distinct()
                    .ToList();

                var volunteers = await _context.Volunteer
                    .Where(v => volunteerIds.Contains(v.MaTNV))
                    .ToListAsync();

                // TNV mới trong tháng (đăng ký lần đầu trong tháng này)
                var newVolunteerIdsThisMonth = registrations
                    .Where(d => d.TrangThai == 1 && d.NgayTao >= firstDayOfMonth)
                    .Select(d => d.MaTNV)
                    .Distinct()
                    .ToList();
                var newVolunteersThisMonth = newVolunteerIdsThisMonth.Count;

                // TNV theo cấp bậc
                var volunteersByRank = volunteers
                    .GroupBy(v => v.CapBac ?? "Chưa xác định")
                    .ToDictionary(g => g.Key, g => g.Count());

                // TNV theo giới tính
                var volunteersByGender = volunteers
                    .Select(v => NormalizeGender(v.GioiTinh))
                    .GroupBy(g => g)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Đảm bảo đủ các key giới tính
                foreach (var key in new[] { "Nam", "Nữ", "Khác" })
                {
                    if (!volunteersByGender.ContainsKey(key))
                        volunteersByGender[key] = 0;
                }

                // Điểm đánh giá trung bình của TNV
                var avgVolunteerRating = volunteers
                    .Where(v => v.DiemTrungBinh.HasValue)
                    .Select(v => (double)v.DiemTrungBinh!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                // Tính số TNV trung bình mỗi sự kiện
                var avgVolunteersPerEvent = events.Any()
                    ? (double)approvedRegistrations / events.Count
                    : 0;

                // Điểm đánh giá trung bình của sự kiện
                var eventRatings = await _context.DanhGia
                    .Where(d => eventIds.Contains(d.MaSuKien))
                    .ToListAsync();
                var avgEventRating = eventRatings.Any()
                    ? eventRatings.Average(r => r.DiemSo)
                    : 0;

                // Thống kê đánh giá tổ chức
                var orgRatings = await _context.DanhGia
                    .Where(d => eventIds.Contains(d.MaSuKien))
                    .ToListAsync();
                var avgRating = orgRatings.Any()
                    ? orgRatings.Average(r => r.DiemSo)
                    : 0;

                // Phân bố điểm đánh giá
                var ratingsDistribution = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    ratingsDistribution[i] = orgRatings.Count(r => Math.Round((double)r.DiemSo) == i);
                }

                return new OrganizationSpecificStatisticsDto
                {
                    OrganizationId = organizationId,
                    OrganizationName = organization.TenToChuc ?? string.Empty,
                    TotalEvents = events.Count,
                    PendingEvents = pendingEvents,
                    ActiveEvents = activeEvents,
                    CompletedEvents = completedEvents,
                    CancelledEvents = cancelledEvents,
                    EventsByMonth = eventsByMonth,
                    EventsByField = eventsByField,
                    AverageVolunteersPerEvent = Math.Round(avgVolunteersPerEvent, 2),
                    AverageEventRating = Math.Round(avgEventRating, 2),
                    TotalRegistrations = registrations.Count,
                    PendingRegistrations = pendingRegistrations,
                    ApprovedRegistrations = approvedRegistrations,
                    RejectedRegistrations = rejectedRegistrations,
                    RegistrationsByMonth = registrationsByMonth,
                    ApprovalRate = Math.Round(approvalRate, 2),
                    TotalVolunteers = volunteers.Count,
                    NewVolunteersThisMonth = newVolunteersThisMonth,
                    VolunteersByRank = volunteersByRank,
                    VolunteersByGender = volunteersByGender,
                    AverageVolunteerRating = Math.Round(avgVolunteerRating, 2),
                    AverageRating = Math.Round(avgRating, 2),
                    RatingsDistribution = ratingsDistribution,
                    TotalRatings = orgRatings.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lấy thống kê tổ chức cụ thể: {ex.Message}");
                throw;
            }
        }
    }
}
