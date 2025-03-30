using ChieuT4_Nhom05_WebQLCF.Models;

namespace ChieuT4_Nhom05_WebQLCF.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        /*Task<Product> GetBySlugAsync(string slug);*/
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<List<Product?>> GetByCategoryIdAsync(int id);
        Task<IEnumerable<Product>> SearchByNameAsync(string name);
        Task ActivateAsync(int id);
        // hien thi danh sach xoa mem 
        Task<IEnumerable<Product>> GetInactiveProductsAsync();
    }

}
