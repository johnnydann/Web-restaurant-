using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]

        /*public string slug { get; set; }*/
        public string Name { get; set; }
        [Range(0.01, 100000000000.00)]
        public decimal Price { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        public string Description { get; set; }
       
        public string? ImageUrl { get; set; }

        //[JsonIgnore]
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }

        //[JsonIgnore]
        public Category? Category { get; set; }
        //public string CategoryName => Category?.Name;
        
        // xóa mềm 
        public bool IsActive { get; set; } = true;

    }
}
