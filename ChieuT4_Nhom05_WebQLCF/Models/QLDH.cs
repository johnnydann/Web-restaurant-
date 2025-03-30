using System.ComponentModel.DataAnnotations;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class QLDH
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 10000.00)]

        public List<OrderDetail> OrderDetails { get; set; } // Danh sách các chi ti?t don hàng
        public decimal Price { get; set; }

        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<ProductImage>? Images { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }
        public QLDH()
        {
            OrderDetails = new List<OrderDetail>();
        }
    }
}
