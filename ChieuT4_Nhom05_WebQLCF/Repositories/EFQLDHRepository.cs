using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using Microsoft.EntityFrameworkCore;

namespace ChieuT4_Nhom05_WebQLCF.Repositories
{
    public class EFQLDHRepository : IQLDHRepository
    {
        private readonly ApplicationDbContext _context;

        public EFQLDHRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QLDH>> GetAllQLDH()
        {
            return await _context.QLDHs.ToListAsync();
        }

        public async Task<QLDH> GetQLDHById(int id)
        {
            return await _context.QLDHs.FindAsync(id);
        }

        public async Task AddQLDH(QLDH qldh)
        {
            _context.Add(qldh);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQLDH(QLDH qldh)
        {
            _context.Update(qldh);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQLDH(int id)
        {
            var qldh = await _context.QLDHs.FindAsync(id);
            _context.QLDHs.Remove(qldh);
            await _context.SaveChangesAsync();
        }
    }
}
