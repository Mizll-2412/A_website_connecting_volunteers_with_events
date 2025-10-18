using khoaluantotnghiep.DTOs;
namespace khoaluantotnghiep.Services
{
    public interface IKyNangService
    {
        Task<IEnumerable<KyNangResponse>> GetAllAsync();
        Task<KyNangResponse> GetByIdAsync(int id);
        Task<KyNangResponse> CreateAsync(CreateKyNangRequest request);
        Task<KyNangResponse> UpdateAsync(int id, UpdateKyNangRequest request);
        Task<bool> DeleteAsync(int id);
    }
}