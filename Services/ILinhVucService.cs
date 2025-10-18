using khoaluantotnghiep.DTOs;
namespace khoaluantotnghiep.Services
{
    public interface ILinhVucService
    {
        Task<IEnumerable<LinhVucResponse>> GetAllAsync();
        Task<LinhVucResponse> GetByIdAsync(int id);
        Task<LinhVucResponse> CreateAsync(CreateLinhVucRequest request);
        Task<LinhVucResponse> UpdateAsync(int id, UpdateLinhVucRequest request);
        Task<bool> DeleteAsync(int id);
    }
}