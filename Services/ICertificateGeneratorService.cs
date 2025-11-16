using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface ICertificateGeneratorService
    {
        Task<string> GeneratePreviewImageAsync(int certificateId);
        Task<byte[]> GenerateCertificateImageAsync(int certificateId);
        Task<byte[]> GenerateCertificatePdfAsync(int certificateId);
        Task<CertificateDataDto> GetCertificateDataAsync(int certificateId);
    }
}

