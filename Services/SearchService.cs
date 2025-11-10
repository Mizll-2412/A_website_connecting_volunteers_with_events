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
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchService> _logger;

        public SearchService(AppDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method để format DateTime thành string yyyy-MM-dd
        private string? FormatDateForResponse(DateTime? date)
        {
            if (date == null) return null;
            return date.Value.ToString("yyyy-MM-dd");
        }

        public async Task<SearchResultPaginationDto<SuKienResponseDto>> SearchEventsAsync(EventSearchFilterDto filter)
        {
            try
            {
                var query = _context.Event
                    .Include(e => e.Organization)
                    .Include(e => e.SuKien_LinhVucs)
                    .ThenInclude(sl => sl.LinhVuc)
                    .Include(e => e.SuKien_KyNangs)
                    .ThenInclude(sk => sk.KyNang)
                    .AsQueryable();

                // Áp dụng các bộ lọc
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    query = query.Where(e =>
                        (e.TenSuKien ?? string.Empty).Contains(filter.Keyword) ||
                        (e.NoiDung ?? string.Empty).Contains(filter.Keyword) ||
                        (e.DiaChi ?? string.Empty).Contains(filter.Keyword));
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(e => e.NgayBatDau >= filter.FromDate);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(e => e.NgayBatDau <= filter.ToDate);
                }

                if (!string.IsNullOrEmpty(filter.Location))
                {
                    query = query.Where(e => (e.DiaChi ?? string.Empty).Contains(filter.Location));
                }

                // Filter theo lĩnh vực
                if (filter.FieldIds != null && filter.FieldIds.Any())
                {
                    query = query.Where(e => e.SuKien_LinhVucs.Any(sl => filter.FieldIds.Contains(sl.MaLinhVuc)));
                }

                // Filter theo kỹ năng
                if (filter.SkillIds != null && filter.SkillIds.Any())
                {
                    query = query.Where(e => e.SuKien_KyNangs.Any(sk => filter.SkillIds.Contains(sk.MaKyNang)));
                }

                // Filter theo tổ chức
                if (filter.OrganizationIds != null && filter.OrganizationIds.Any())
                {
                    query = query.Where(e => filter.OrganizationIds.Contains(e.MaToChuc));
                }

                if (filter.OnlyVerifiedOrganizations.HasValue && filter.OnlyVerifiedOrganizations.Value)
                {
                    query = query.Where(e => e.Organization.TrangThaiXacMinh == 1); // 1: Đã xác minh
                }

                // Filter theo trạng thái
                if (filter.StatusIds != null && filter.StatusIds.Any())
                {
                    var now = DateTime.Now;
                    
                    // 0: Chưa diễn ra, 1: Đang diễn ra, 2: Đã kết thúc, 3: Đã hủy
                    query = query.Where(e => 
                        (filter.StatusIds.Contains(0) && e.NgayBatDau > now) ||
                        (filter.StatusIds.Contains(1) && e.NgayBatDau <= now && (!e.NgayKetThuc.HasValue || e.NgayKetThuc >= now)) ||
                        (filter.StatusIds.Contains(2) && e.NgayKetThuc.HasValue && e.NgayKetThuc < now) ||
                        (filter.StatusIds.Contains(3) && e.TrangThai == "2")
                    );
                }

                // Filter theo số lượng TNV
                if (filter.MinVolunteers.HasValue || filter.MaxVolunteers.HasValue)
                {
                    // Lấy số lượng tình nguyện viên đã đăng ký cho từng sự kiện
                    var eventVolunteerCounts = await _context.DonDangKy
                        .Where(d => d.TrangThai == 1) // Chỉ đếm đơn đã duyệt
                        .GroupBy(d => d.MaSuKien)
                        .Select(g => new { EventId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.EventId, x => x.Count);

                    var eventIdsForFilter = query.Select(e => e.MaSuKien).ToList();
                    var filteredEventIds = eventIdsForFilter
                        .Where(id => 
                            (!filter.MinVolunteers.HasValue || 
                             (eventVolunteerCounts.ContainsKey(id) && eventVolunteerCounts[id] >= filter.MinVolunteers)) &&
                            (!filter.MaxVolunteers.HasValue || 
                             (eventVolunteerCounts.ContainsKey(id) && eventVolunteerCounts[id] <= filter.MaxVolunteers))
                        )
                        .ToList();

                    query = query.Where(e => filteredEventIds.Contains(e.MaSuKien));
                }

                // Tính tổng số bản ghi
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                // Sắp xếp và phân trang
                IOrderedQueryable<SuKien> orderedQuery;
                switch (filter.SortBy?.ToLower())
                {
                    case "date_asc":
                        orderedQuery = query.OrderBy(e => e.NgayBatDau);
                        break;
                    case "popularity":
                        // Sắp xếp theo số lượng đăng ký
                        var popularEvents = await _context.DonDangKy
                            .Where(d => d.TrangThai == 1) // Đã duyệt
                            .GroupBy(d => d.MaSuKien)
                            .Select(g => new { EventId = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToListAsync();
                        
                        var popularEventIds = popularEvents.Select(pe => pe.EventId).ToList();
                        var remainingEventIds = query.Select(e => e.MaSuKien)
                                                    .Where(id => !popularEventIds.Contains(id))
                                                    .ToList();
                        
                        // Kết hợp danh sách để giữ thứ tự
                        popularEventIds.AddRange(remainingEventIds);
                        
                        // Sắp xếp theo thứ tự trong danh sách popularEventIds
                        orderedQuery = query.OrderBy(e => popularEventIds.IndexOf(e.MaSuKien));
                        break;
                    case "distance":
                        // Nếu có tọa độ, sắp xếp theo khoảng cách
                        if (filter.Latitude.HasValue && filter.Longitude.HasValue)
                        {
                            // Giả lập sắp xếp theo khoảng cách (thực tế cần tính khoảng cách dựa trên tọa độ)
                            orderedQuery = query.OrderBy(e => e.DiaChi);
                        }
                        else
                        {
                            orderedQuery = query.OrderByDescending(e => e.NgayBatDau);
                        }
                        break;
                    case "date_desc":
                    default:
                        orderedQuery = query.OrderByDescending(e => e.NgayBatDau);
                        break;
                }

                // Áp dụng phân trang
                var pagedEvents = await orderedQuery
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Chuyển đổi dữ liệu sang DTO
                // Lấy danh sách ID sự kiện để đếm số đăng ký
                var eventIdsForCount = pagedEvents.Select(e => e.MaSuKien).ToList();
                
                // Đếm số đơn đăng ký đã duyệt theo từng sự kiện (TrangThai = 1)
                var registrationCounts = await _context.DonDangKy
                    .Where(d => eventIdsForCount.Contains(d.MaSuKien) && d.TrangThai == 1)
                    .GroupBy(d => d.MaSuKien)
                    .Select(g => new { MaSuKien = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.MaSuKien, x => x.Count);

                var eventDtos = pagedEvents.Select(e =>
                {
                    var organization = e.Organization;
                    return new SuKienResponseDto
                    {
                        MaSuKien = e.MaSuKien,
                        TenSuKien = e.TenSuKien ?? string.Empty,
                        MoTa = e.NoiDung ?? string.Empty,
                        NgayBatDau = e.NgayBatDau,
                        NgayKetThuc = e.NgayKetThuc,
                        DiaChi = e.DiaChi ?? string.Empty,
                        HinhAnh = e.HinhAnh ?? string.Empty,
                        SoLuong = e.SoLuong,
                        MaToChuc = e.MaToChuc,
                        TenToChuc = organization?.TenToChuc ?? string.Empty,
                        TrangThaiXacMinhToChuc = organization?.TrangThaiXacMinh == 1,
                        TrangThai = int.TryParse(e.TrangThai, out int trangThai) ? trangThai : 0,
                        LinhVucs = e.SuKien_LinhVucs.Select(sl => new LinhVucDto
                        {
                            MaLinhVuc = sl.MaLinhVuc,
                            TenLinhVuc = sl.LinhVuc?.TenLinhVuc ?? string.Empty
                        }).ToList(),
                        KyNangs = e.SuKien_KyNangs.Select(sk => new KyNangDto
                        {
                            MaKyNang = sk.MaKyNang,
                            TenKyNang = sk.KyNang?.TenKyNang ?? string.Empty
                        }).ToList(),
                        SoLuongDaDangKy = registrationCounts.ContainsKey(e.MaSuKien) ? registrationCounts[e.MaSuKien] : 0
                    };
                }).ToList();

                // Tạo các facet cho bộ lọc
                var facets = new Dictionary<string, dynamic>
                {
                    { "fields", await _context.LinhVuc.Select(f => new { Id = f.MaLinhVuc, Name = f.TenLinhVuc }).ToListAsync() },
                    { "skills", await _context.KyNang.Select(s => new { Id = s.MaKyNang, Name = s.TenKyNang }).ToListAsync() },
                    { "organizations", await _context.Organization.Select(o => new { Id = o.MaToChuc, Name = o.TenToChuc, Verified = o.TrangThaiXacMinh == 1 }).ToListAsync() },
                    { "statuses", new List<object> 
                        {
                            new { Id = 0, Name = "Chưa diễn ra" },
                            new { Id = 1, Name = "Đang diễn ra" },
                            new { Id = 2, Name = "Đã kết thúc" },
                            new { Id = 3, Name = "Đã hủy" }
                        } 
                    }
                };

                return new SearchResultPaginationDto<SuKienResponseDto>
                {
                    Items = eventDtos,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    Facets = facets
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tìm kiếm sự kiện: {ex.Message}");
                throw;
            }
        }

        public async Task<SearchResultPaginationDto<TinhNguyenVienResponseDto>> SearchVolunteersAsync(VolunteerSearchFilterDto filter)
        {
            try
            {
                var query = _context.Volunteer
                    .Include(v => v.TinhNguyenVien_LinhVucs)
                    .ThenInclude(vl => vl.LinhVuc)
                    .Include(v => v.TinhNguyenVien_KyNangs)
                    .ThenInclude(vk => vk.KyNang)
                    .AsQueryable();

                // Áp dụng các bộ lọc
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    query = query.Where(v =>
                        (v.HoTen ?? string.Empty).Contains(filter.Keyword) ||
                        (v.Email ?? string.Empty).Contains(filter.Keyword) ||
                        (v.GioiThieu ?? string.Empty).Contains(filter.Keyword));
                }

                // Filter theo độ tuổi
                if (filter.MinAge.HasValue || filter.MaxAge.HasValue)
                {
                    var currentDate = DateTime.Today;
                    
                    if (filter.MinAge.HasValue)
                    {
                        var maxBirthDate = currentDate.AddYears(-filter.MinAge.Value);
                        query = query.Where(v => v.NgaySinh.HasValue && v.NgaySinh.Value <= maxBirthDate);
                    }
                    
                    if (filter.MaxAge.HasValue)
                    {
                        var minBirthDate = currentDate.AddYears(-filter.MaxAge.Value - 1).AddDays(1);
                        query = query.Where(v => v.NgaySinh.HasValue && v.NgaySinh.Value >= minBirthDate);
                    }
                }

                // Filter theo giới tính
                if (!string.IsNullOrEmpty(filter.Gender))
                {
                    query = query.Where(v => v.GioiTinh == filter.Gender);
                }

                // Filter theo địa điểm
                if (!string.IsNullOrEmpty(filter.Location))
                {
                    query = query.Where(v => (v.DiaChi ?? string.Empty).Contains(filter.Location));
                }

                // Filter theo lĩnh vực
                if (filter.FieldIds != null && filter.FieldIds.Any())
                {
                    query = query.Where(v => v.TinhNguyenVien_LinhVucs.Any(vl => filter.FieldIds.Contains(vl.MaLinhVuc)));
                }

                // Filter theo kỹ năng
                if (filter.SkillIds != null && filter.SkillIds.Any())
                {
                    query = query.Where(v => v.TinhNguyenVien_KyNangs.Any(vk => filter.SkillIds.Contains(vk.MaKyNang)));
                }

                // Filter theo điểm uy tín
                if (filter.MinRating.HasValue)
                {
                    query = query.Where(v => v.DiemTrungBinh.HasValue && v.DiemTrungBinh.Value >= filter.MinRating);
                }

                if (filter.MaxRating.HasValue)
                {
                    query = query.Where(v => v.DiemTrungBinh.HasValue && v.DiemTrungBinh.Value <= filter.MaxRating);
                }

                // Filter theo cấp bậc
                if (filter.RankNames != null && filter.RankNames.Any())
                {
                    query = query.Where(v => v.CapBac != null && filter.RankNames.Contains(v.CapBac));
                }

                // Filter theo số sự kiện tham gia
                if (filter.MinEvents.HasValue)
                {
                    query = query.Where(v => v.TongSuKienThamGia.HasValue && v.TongSuKienThamGia.Value >= filter.MinEvents);
                }

                // Tính tổng số bản ghi
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                // Sắp xếp và phân trang
                IOrderedQueryable<TinhNguyenVien> orderedQuery;
                switch (filter.SortBy?.ToLower())
                {
                    case "events_desc":
                        orderedQuery = query.OrderByDescending(v => v.TongSuKienThamGia);
                        break;
                    case "name_asc":
                        orderedQuery = query.OrderBy(v => v.HoTen);
                        break;
                    case "rating_desc":
                    default:
                        orderedQuery = query.OrderByDescending(v => v.DiemTrungBinh);
                        break;
                }

                // Áp dụng phân trang
                var pagedVolunteers = await orderedQuery
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Chuyển đổi dữ liệu sang DTO
                var volunteerDtos = new List<TinhNguyenVienResponseDto>();

                foreach (var v in pagedVolunteers)
                {
                    var linhVucIds = v.TinhNguyenVien_LinhVucs?
                        .Select(vl => vl.MaLinhVuc).ToList() ?? new List<int>();

                    var kyNangIds = v.TinhNguyenVien_KyNangs?
                        .Select(vk => vk.MaKyNang).ToList() ?? new List<int>();

                    var linhVucs = linhVucIds.Count == 0
                        ? new List<LinhVucDto>()
                        : await _context.LinhVuc
                            .Where(l => linhVucIds.Contains(l.MaLinhVuc))
                            .Select(l => new LinhVucDto
                            {
                                MaLinhVuc = l.MaLinhVuc,
                                TenLinhVuc = l.TenLinhVuc ?? string.Empty
                            })
                            .ToListAsync();

                    var kyNangs = kyNangIds.Count == 0
                        ? new List<KyNangDto>()
                        : await _context.KyNang
                            .Where(k => kyNangIds.Contains(k.MaKyNang))
                            .Select(k => new KyNangDto
                            {
                                MaKyNang = k.MaKyNang,
                                TenKyNang = k.TenKyNang ?? string.Empty
                            })
                            .ToListAsync();

                    volunteerDtos.Add(new TinhNguyenVienResponseDto
                    {
                        MaTNV = v.MaTNV,
                        MaTaiKhoan = v.MaTaiKhoan,
                        HoTen = v.HoTen ?? string.Empty,
                        NgaySinh = FormatDateForResponse(v.NgaySinh),
                        GioiTinh = v.GioiTinh,
                        Email = v.Email ?? string.Empty,
                        CCCD = v.CCCD,
                        SoDienThoai = v.SoDienThoai,
                        DiaChi = v.DiaChi ?? string.Empty,
                        GioiThieu = v.GioiThieu ?? string.Empty,
                        AnhDaiDien = v.AnhDaiDien,
                        DiemTrungBinh = v.DiemTrungBinh,
                        CapBac = v.CapBac,
                        TongSuKienThamGia = v.TongSuKienThamGia,
                        LinhVucIds = linhVucIds,
                        KyNangIds = kyNangIds,
                        LinhVucs = linhVucs,
                        KyNangs = kyNangs
                    });
                }

                // Tạo các facet cho bộ lọc
                var facets = new Dictionary<string, dynamic>
                {
                    { "fields", await _context.LinhVuc.Select(f => new { Id = f.MaLinhVuc, Name = f.TenLinhVuc }).ToListAsync() },
                    { "skills", await _context.KyNang.Select(s => new { Id = s.MaKyNang, Name = s.TenKyNang }).ToListAsync() },
                    { "ranks", await _context.Volunteer
                                .Where(v => !string.IsNullOrEmpty(v.CapBac))
                                .Select(v => v.CapBac)
                                .Distinct()
                                .ToListAsync() },
                    { "genders", new List<string> { "Nam", "Nữ", "Khác" } }
                };

                return new SearchResultPaginationDto<TinhNguyenVienResponseDto>
                {
                    Items = volunteerDtos,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    Facets = facets
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tìm kiếm tình nguyện viên: {ex.Message}");
                throw;
            }
        }

        public async Task<SearchResultPaginationDto<ToChucResponseDto>> SearchOrganizationsAsync(OrganizationSearchFilterDto filter)
        {
            try
            {
                var query = _context.Organization.AsQueryable();

                // Áp dụng các bộ lọc
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    query = query.Where(o =>
                        (o.TenToChuc ?? string.Empty).Contains(filter.Keyword) ||
                        (o.GioiThieu ?? string.Empty).Contains(filter.Keyword) ||
                        (o.DiaChi ?? string.Empty).Contains(filter.Keyword) ||
                        (o.Email ?? string.Empty).Contains(filter.Keyword));
                }

                // Filter theo địa điểm
                if (!string.IsNullOrEmpty(filter.Location))
                {
                    query = query.Where(o => (o.DiaChi ?? string.Empty).Contains(filter.Location));
                }

                // Filter theo trạng thái xác minh
                if (filter.VerificationStatus.HasValue)
                {
                    query = query.Where(o => o.TrangThaiXacMinh == filter.VerificationStatus);
                }

                // Filter theo điểm uy tín
                if (filter.MinRating.HasValue)
                {
                    query = query.Where(o => o.DiemTrungBinh.HasValue && o.DiemTrungBinh.Value >= filter.MinRating);
                }

                if (filter.MaxRating.HasValue)
                {
                    query = query.Where(o => o.DiemTrungBinh.HasValue && o.DiemTrungBinh.Value <= filter.MaxRating);
                }

                // Filter theo số sự kiện đã tổ chức
                if (filter.MinEvents.HasValue)
                {
                    // Lấy số sự kiện đã tổ chức của từng tổ chức
                    var orgEventCounts = await _context.Event
                        .GroupBy(e => e.MaToChuc)
                        .Select(g => new { OrgId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.OrgId, x => x.Count);

                    var orgIds = query.Select(o => o.MaToChuc).ToList();
                    var filteredOrgIds = orgIds
                        .Where(id => orgEventCounts.ContainsKey(id) && orgEventCounts[id] >= filter.MinEvents)
                        .ToList();

                    query = query.Where(o => filteredOrgIds.Contains(o.MaToChuc));
                }

                // Filter theo lĩnh vực
                if (filter.FieldIds != null && filter.FieldIds.Any())
                {
                    // Lấy các tổ chức có sự kiện thuộc lĩnh vực cần tìm
                    var orgIdsWithField = await _context.Event
                        .Include(e => e.SuKien_LinhVucs)
                        .Where(e => e.SuKien_LinhVucs.Any(sl => filter.FieldIds.Contains(sl.MaLinhVuc)))
                        .Select(e => e.MaToChuc)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(o => orgIdsWithField.Contains(o.MaToChuc));
                }

                // Tính tổng số bản ghi
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

                // Sắp xếp và phân trang
                IOrderedQueryable<ToChuc> orderedQuery;
                switch (filter.SortBy?.ToLower())
                {
                    case "events_desc":
                        // Lấy số lượng sự kiện của mỗi tổ chức
                        var orgEvents = await _context.Event
                            .GroupBy(e => e.MaToChuc)
                            .Select(g => new { OrgId = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToListAsync();
                        
                        var orgIds = orgEvents.Select(oe => oe.OrgId).ToList();
                        var remainingOrgIds = query.Select(o => o.MaToChuc)
                                                 .Where(id => !orgIds.Contains(id))
                                                 .ToList();
                        
                        // Kết hợp danh sách để giữ thứ tự
                        orgIds.AddRange(remainingOrgIds);
                        
                        // Sắp xếp theo thứ tự trong danh sách orgIds
                        orderedQuery = query.OrderBy(o => orgIds.IndexOf(o.MaToChuc));
                        break;
                    case "name_asc":
                        orderedQuery = query.OrderBy(o => o.TenToChuc ?? string.Empty);
                        break;
                    case "rating_desc":
                    default:
                        orderedQuery = query.OrderByDescending(o => o.DiemTrungBinh);
                        break;
                }

                // Áp dụng phân trang
                var pagedOrganizations = await orderedQuery
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Chuyển đổi dữ liệu sang DTO
                var organizationDtos = pagedOrganizations.Select(o => new ToChucResponseDto
                {
                    MaToChuc = o.MaToChuc,
                    MaTaiKhoan = o.MaTaiKhoan,
                    TenToChuc = o.TenToChuc ?? string.Empty,
                    MoTa = o.GioiThieu,
                    Email = o.Email ?? string.Empty,
                    SoDienThoai = o.SoDienThoai,
                    DiaChi = o.DiaChi ?? string.Empty,
                    Website = null, // o.Website không tồn tại
                    Logo = o.AnhDaiDien,
                    TrangThaiXacMinh = o.TrangThaiXacMinh,
                    DiemTrungBinh = o.DiemTrungBinh,
                    LyDoTuChoi = o.LyDoTuChoi
                }).ToList();

                // Tạo các facet cho bộ lọc
                var facets = new Dictionary<string, dynamic>
                {
                    { "fields", await _context.LinhVuc.Select(f => new { Id = f.MaLinhVuc, Name = f.TenLinhVuc }).ToListAsync() },
                    { "verificationStatuses", new List<object> 
                        {
                            new { Id = 0, Name = "Chưa xác minh" },
                            new { Id = 1, Name = "Đã xác minh" },
                            new { Id = 2, Name = "Đang chờ xác minh" }
                        } 
                    }
                };

                return new SearchResultPaginationDto<ToChucResponseDto>
                {
                    Items = organizationDtos,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    Facets = facets
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tìm kiếm tổ chức: {ex.Message}");
                throw;
            }
        }

        public async Task<dynamic> SearchAllAsync(string keyword, int page = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    throw new ArgumentException("Từ khóa tìm kiếm không được để trống");
                }

                // Tìm kiếm sự kiện
                var events = await _context.Event
                    .Where(e => (e.TenSuKien ?? string.Empty).Contains(keyword) ||
                                (e.NoiDung ?? string.Empty).Contains(keyword) ||
                                (e.DiaChi ?? string.Empty).Contains(keyword))
                    .Include(e => e.Organization)
                    .OrderByDescending(e => e.NgayBatDau)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        Type = "event",
                        Id = e.MaSuKien,
                        Title = e.TenSuKien ?? string.Empty,
                        Description = e.NoiDung ?? string.Empty,
                        StartDate = e.NgayBatDau,
                        EndDate = e.NgayKetThuc,
                        Location = e.DiaChi ?? string.Empty,
                        Image = e.HinhAnh,
                        OrganizationName = e.Organization != null ? (e.Organization.TenToChuc ?? string.Empty) : string.Empty
                    })
                    .ToListAsync();

                // Tìm kiếm tình nguyện viên
                var volunteers = await _context.Volunteer
                    .Where(v => (v.HoTen ?? string.Empty).Contains(keyword) || (v.GioiThieu ?? string.Empty).Contains(keyword))
                    .OrderByDescending(v => v.DiemTrungBinh)
                    .Take(pageSize)
                    .Select(v => new
                    {
                        Type = "volunteer",
                        Id = v.MaTNV,
                        Name = v.HoTen ?? string.Empty,
                        Description = v.GioiThieu ?? string.Empty,
                        Image = v.AnhDaiDien,
                        Rating = v.DiemTrungBinh,
                        EventsCount = v.TongSuKienThamGia,
                        Rank = v.CapBac
                    })
                    .ToListAsync();

                // Tìm kiếm tổ chức
                var organizations = await _context.Organization
                    .Where(o => (o.TenToChuc ?? string.Empty).Contains(keyword) ||
                                (o.GioiThieu ?? string.Empty).Contains(keyword) ||
                                (o.DiaChi ?? string.Empty).Contains(keyword))
                    .OrderByDescending(o => o.DiemTrungBinh)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        Type = "organization",
                        Id = o.MaToChuc,
                        Name = o.TenToChuc ?? string.Empty,
                        Description = o.GioiThieu ?? string.Empty,
                        Image = o.AnhDaiDien,
                        Rating = o.DiemTrungBinh,
                        VerificationStatus = o.TrangThaiXacMinh
                    })
                    .ToListAsync();

                // Kết hợp tất cả kết quả và sắp xếp
                var allResults = new List<object>();
                allResults.AddRange(events);
                allResults.AddRange(volunteers);
                allResults.AddRange(organizations);

                return new
                {
                    Events = events,
                    Volunteers = volunteers,
                    Organizations = organizations,
                    Combined = allResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tìm kiếm: {ex.Message}");
                throw;
            }
        }
    }
}
