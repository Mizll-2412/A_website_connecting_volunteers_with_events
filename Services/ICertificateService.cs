using System.Collections.Generic;
using System.Threading.Tasks;
using khoaluantotnghiep.DTOs;
using Microsoft.AspNetCore.Http;

namespace khoaluantotnghiep.Services
{
    public interface ICertificateService
    {
        // Các phương thức cho mẫu giấy chứng nhận
        Task<CertificateSampleDto> CreateCertificateSampleAsync(CreateCertificateSampleDto createDto);
        Task<CertificateSampleDto> GetCertificateSampleByIdAsync(int maMau);
        Task<List<CertificateSampleDto>> GetAllCertificateSamplesAsync();
        Task<List<CertificateSampleDto>> GetCertificateSamplesByEventAsync(int maSuKien);
        Task<bool> DeleteCertificateSampleAsync(int maMau);
        Task<CertificateSampleDto> SetDefaultCertificateSampleAsync(int maMau);
        Task<string> UploadCertificateSampleFileAsync(IFormFile file);

        // Các phương thức cho giấy chứng nhận cụ thể
        Task<CertificateDto> IssueCertificateAsync(IssueCertificateDto issueDto);
        Task<CertificateDto> GetCertificateByIdAsync(int maGiayChungNhan);
        Task<List<CertificateDto>> GetCertificatesByVolunteerAsync(int maTNV);
        Task<List<CertificateDto>> GetCertificatesByEventAsync(int maSuKien);
        Task<List<CertificateDto>> GetCertificatesWithFilterAsync(CertificateFilterDto filter);
        Task<bool> DeleteCertificateAsync(int maGiayChungNhan);
        Task<string> UploadCertificateFileAsync(IFormFile file);
        
        // Phương thức phát hành hàng loạt
        Task<List<CertificateDto>> IssueCertificatesToEventParticipantsAsync(int maSuKien, int maMau);
        
        // Template config methods
        Task<CertificateSampleDto> UpdateTemplateConfigAsync(int maMau, TemplateConfigDto configDto);
        Task<TemplateConfigDto> GetTemplateConfigAsync(int maMau);
    }
}
