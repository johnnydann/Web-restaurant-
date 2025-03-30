using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using Microsoft.EntityFrameworkCore;

namespace ChieuT4_Nhom05_WebQLCF.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Product>?> GetByCategoryIdAsync(int id)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                        .Include(p => p.Category) // Nạp thông tin danh mục
                        .FirstOrDefaultAsync(p => p.Id == id);
        }

        /*public async Task<Product> GetBySlugAsync(string slug)
        {
            return await _context.Products
                        .Include(p => p.Category) // Nạp thông tin danh mục
                        .FirstOrDefaultAsync(p => p.slug == slug);
        }*/

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Enumerable.Empty<Product>();
            }

            var searchTerms = name.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

            return await _context.Products
                .Where(p => searchTerms.All(term => p.Name.Contains(term)) && p.IsActive)
                .ToListAsync();
        }
        //xoa mem 
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
        // hien thi lai sau khi xoa mem
        public async Task ActivateAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }
        // hien thi ds sau khi xoa mem
        public async Task<IEnumerable<Product>> GetInactiveProductsAsync()
        {
            return await _context.Products.Where(p => !p.IsActive).ToListAsync();
        }
    }
}
