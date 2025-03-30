using ChieuT4_Nhom05_WebQLCF.Models;
namespace ChieuT4_Nhom05_WebQLCF.Repositories
{
    public interface IQLDHRepository
    {
        Task<IEnumerable<QLDH>> GetAllQLDH();
        Task<QLDH> GetQLDHById(int id);
        Task AddQLDH(QLDH qldh);
        Task UpdateQLDH(QLDH qldh);
        Task DeleteQLDH(int id);
    }
}
