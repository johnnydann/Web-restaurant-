using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required] 
        public string Slug { get; set; }

        [JsonIgnore]
        public List<Product>? Products { get; set; }

    }
}
