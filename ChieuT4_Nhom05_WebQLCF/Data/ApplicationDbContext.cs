using ChieuT4_Nhom05_WebQLCF.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChieuT4_Nhom05_WebQLCF.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
        options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Đảm bảo cấu trúc chuẩn của Identity
            modelBuilder.Entity<CartItem>().HasKey(ci => ci.IdCartItem); // Xác định khóa chính
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<QLDH> QLDHs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set;}
        public DbSet<CartItem> CartItems { get; set; }
    }
}
