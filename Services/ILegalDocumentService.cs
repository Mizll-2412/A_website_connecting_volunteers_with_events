using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface ILegalDocumentService
    {
        Task<GiayToPhapLyResponseDto> UploadGiayToAsync(UploadDocument uploadDto);
        Task<List<GiayToPhapLyResponseDto>> GetGiayToByToChucAsync(int maToChuc);
        Task<bool> DeleteGiayToAsync(int maGiayTo);
        Task<ToChucXacMinhResponseDto> XacMinhToChucAsync(int maToChuc, XacMinhToChucDto xacMinhDto);
        Task<List<ToChucXacMinhResponseDto>> GetDanhSachChoXacMinhAsync();
        Task<ToChucXacMinhResponseDto> GetThongTinXacMinhAsync(int maToChuc);
    }
}