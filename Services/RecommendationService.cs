using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using khoaluantotnghiep.Data;
using khoaluantotnghiep.DTOs;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(AppDbContext context, ILogger<RecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các sự kiện được gợi ý cho tình nguyện viên
        /// </summary>
        public async Task<List<EventRecommendationDto>> GetRecommendedEventsAsync(RecommendationRequestDto request)
        {
            try
            {
                // 1. Lấy thông tin tình nguyện viên
                var tnv = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_KyNangs)
                        .ThenInclude(k => k.KyNang)
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                        .ThenInclude(l => l.LinhVuc)
                    .FirstOrDefaultAsync(t => t.MaTNV == request.MaTNV);

                if (tnv == null)
                {
                    throw new Exception("Không tìm thấy tình nguyện viên");
                }

                // 2. Lấy danh sách sự kiện đang và sắp diễn ra
                var now = DateTime.Now;
                var events = await _context.Event
                    .Include(e => e.Organization)
                    .Include(e => e.SuKien_KyNangs)
                        .ThenInclude(sk => sk.KyNang)
                    .Include(e => e.SuKien_LinhVucs)
                        .ThenInclude(sl => sl.LinhVuc)
                    .Where(e => e.NgayKetThuc >= now) // Chỉ lấy sự kiện chưa kết thúc
                    .ToListAsync();

                // 3. Lấy danh sách sự kiện đã đăng ký
                var registeredEvents = await _context.DonDangKy
                    .Where(d => d.MaTNV == request.MaTNV)
                    .Select(d => d.MaSuKien)
                    .ToListAsync();

                // 4. Loại bỏ các sự kiện đã đăng ký
                events = events.Where(e => !registeredEvents.Contains(e.MaSuKien)).ToList();

                // 5. Tính điểm phù hợp cho từng sự kiện
                var results = new List<EventRecommendationDto>();
                foreach (var ev in events)
                {
                    var matchScore = await CalculateMatchScoreAsync(
                        ev.MaSuKien, 
                        request.MaTNV, 
                        request.LocationWeight, 
                        request.SkillWeight, 
                        request.InterestWeight);

                    results.Add(new EventRecommendationDto
                    {
                        MaSuKien = ev.MaSuKien,
                        TenSuKien = ev.TenSuKien,
                        MoTa = ev.NoiDung,
                        NgayBatDau = ev.NgayBatDau,
                        NgayKetThuc = ev.NgayKetThuc,
                        DiaChi = ev.DiaChi,
                        HinhAnh = ev.HinhAnh,
                        MaToChuc = ev.MaToChuc,
                        TenToChuc = ev.Organization?.TenToChuc,
                        DiemTrungBinhToChuc = ev.Organization?.DiemTrungBinh,
                        KyNangs = ev.SuKien_KyNangs?.Select(k => k.KyNang?.TenKyNang).Where(n => n != null).ToList(),
                        LinhVucs = ev.SuKien_LinhVucs?.Select(l => l.LinhVuc?.TenLinhVuc).Where(n => n != null).ToList(),
                        MatchScore = matchScore
                    });
                }

                // 6. Lọc theo lĩnh vực ưu tiên nếu có
                if (request.LinhVucPreferences != null && request.LinhVucPreferences.Any())
                {
                    foreach (var result in results)
                    {
                        var eventLinhVucs = await _context.SuKien_LinhVuc
                            .Where(sl => sl.MaSuKien == result.MaSuKien)
                            .Select(sl => sl.MaLinhVuc)
                            .ToListAsync();

                        // Tăng điểm nếu sự kiện có lĩnh vực ưu tiên
                        var matchingPreferences = eventLinhVucs.Intersect(request.LinhVucPreferences).Count();
                        if (matchingPreferences > 0)
                        {
                            // Tăng điểm dựa trên số lượng lĩnh vực khớp
                            result.MatchScore += (decimal)(matchingPreferences * 0.1);
                        }
                    }
                }

                // 7. Sắp xếp theo điểm phù hợp và giới hạn số lượng kết quả
                var maxResults = request.MaxResults ?? 10;
                return results
                    .OrderByDescending(r => r.MatchScore)
                    .Take(maxResults)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi lấy sự kiện gợi ý: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tính điểm phù hợp giữa tình nguyện viên và sự kiện
        /// </summary>
        public async Task<decimal> CalculateMatchScoreAsync(
            int maSuKien, 
            int maTNV, 
            double? locationWeight = 0.3, 
            double? skillWeight = 0.4, 
            double? interestWeight = 0.3)
        {
            try
            {
                // 1. Lấy thông tin sự kiện
                var suKien = await _context.Event
                    .Include(s => s.SuKien_KyNangs)
                    .Include(s => s.SuKien_LinhVucs)
                    .FirstOrDefaultAsync(s => s.MaSuKien == maSuKien);

                if (suKien == null)
                {
                    throw new Exception("Không tìm thấy sự kiện");
                }

                // 2. Lấy thông tin tình nguyện viên
                var tnv = await _context.Volunteer
                    .Include(t => t.TinhNguyenVien_KyNangs)
                    .Include(t => t.TinhNguyenVien_LinhVucs)
                    .FirstOrDefaultAsync(t => t.MaTNV == maTNV);

                if (tnv == null)
                {
                    throw new Exception("Không tìm thấy tình nguyện viên");
                }

                // 3. Tính điểm phù hợp về kỹ năng (skill match)
                double skillScore = 0;
                if (suKien.SuKien_KyNangs != null && suKien.SuKien_KyNangs.Any() && 
                    tnv.TinhNguyenVien_KyNangs != null && tnv.TinhNguyenVien_KyNangs.Any())
                {
                    var eventSkills = suKien.SuKien_KyNangs.Select(s => s.MaKyNang).ToList();
                    var tnvSkills = tnv.TinhNguyenVien_KyNangs.Select(t => t.MaKyNang).ToList();
                    
                    // Tính tỷ lệ kỹ năng khớp
                    var matchingSkills = eventSkills.Intersect(tnvSkills).Count();
                    
                    // Nếu sự kiện yêu cầu kỹ năng
                    if (eventSkills.Any())
                    {
                        skillScore = (double)matchingSkills / eventSkills.Count();
                    }
                    else
                    {
                        skillScore = 0.5; // Điểm trung bình nếu sự kiện không yêu cầu kỹ năng
                    }
                }
                else if (!suKien.SuKien_KyNangs.Any())
                {
                    // Nếu sự kiện không yêu cầu kỹ năng, mặc định cho điểm tốt
                    skillScore = 0.8;
                }
                else
                {
                    // Nếu TNV không có kỹ năng nào, cho điểm thấp
                    skillScore = 0.2;
                }

                // 4. Tính điểm phù hợp về lĩnh vực quan tâm (interest match)
                double interestScore = 0;
                if (suKien.SuKien_LinhVucs != null && suKien.SuKien_LinhVucs.Any() && 
                    tnv.TinhNguyenVien_LinhVucs != null && tnv.TinhNguyenVien_LinhVucs.Any())
                {
                    var eventFields = suKien.SuKien_LinhVucs.Select(s => s.MaLinhVuc).ToList();
                    var tnvFields = tnv.TinhNguyenVien_LinhVucs.Select(t => t.MaLinhVuc).ToList();
                    
                    // Tính tỷ lệ lĩnh vực khớp
                    var matchingFields = eventFields.Intersect(tnvFields).Count();
                    
                    if (eventFields.Any())
                    {
                        interestScore = (double)matchingFields / eventFields.Count();
                    }
                    else
                    {
                        interestScore = 0.5;
                    }
                }
                else if (!suKien.SuKien_LinhVucs.Any())
                {
                    // Nếu sự kiện không yêu cầu lĩnh vực, mặc định cho điểm khá
                    interestScore = 0.7;
                }
                else
                {
                    // Nếu TNV không có lĩnh vực quan tâm nào, cho điểm thấp
                    interestScore = 0.3;
                }

                // 5. Tính điểm phù hợp về địa điểm (location match)
                // Đơn giản hóa: Nếu địa chỉ TNV và địa chỉ sự kiện có cùng địa danh, điểm cao
                double locationScore = 0.5; // Mặc định điểm trung bình
                
                if (!string.IsNullOrEmpty(tnv.DiaChi) && !string.IsNullOrEmpty(suKien.DiaChi))
                {
                    // Tách địa chỉ thành các phần và tìm sự trùng khớp
                    var tnvAddressParts = tnv.DiaChi.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var eventAddressParts = suKien.DiaChi.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    var commonParts = tnvAddressParts.Intersect(eventAddressParts).Count();
                    
                    if (commonParts > 0)
                    {
                        // Điểm tăng theo số phần trùng khớp
                        locationScore = Math.Min(0.2 + commonParts * 0.2, 1.0);
                    }
                }

                // 6. Tính tổng điểm phù hợp (có trọng số)
                var totalScore = (skillScore * (skillWeight ?? 0.4)) + 
                                (interestScore * (interestWeight ?? 0.3)) + 
                                (locationScore * (locationWeight ?? 0.3));
                
                // Giới hạn trong khoảng 0-1
                totalScore = Math.Max(0, Math.Min(totalScore, 1));

                // 7. Thêm điểm thưởng dựa vào điểm đánh giá của tổ chức
                var org = await _context.Organization.FindAsync(suKien.MaToChuc);
                if (org != null && org.DiemTrungBinh.HasValue && org.DiemTrungBinh.Value > 3.5M)
                {
                    // Thêm điểm thưởng cho tổ chức có đánh giá tốt
                    var orgBonus = (double)(org.DiemTrungBinh.Value - 3.5M) / 5; // Tối đa thêm 30%
                    totalScore = Math.Min(totalScore + orgBonus, 1.0);
                }

                // 8. Đối với sự kiện sắp diễn ra, thêm điểm ưu tiên
                if (suKien.NgayBatDau.HasValue && suKien.NgayBatDau.Value <= DateTime.Now.AddDays(3))
                {
                    totalScore = Math.Min(totalScore + 0.1, 1.0); // Thêm 10% cho sự kiện sắp diễn ra
                }

                // Trả về điểm phù hợp (chuyển về thang điểm 0-10 để dễ hiểu)
                return (decimal)(totalScore * 10);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi tính điểm phù hợp: {ex.Message}");
                throw;
            }
        }
    }
}
