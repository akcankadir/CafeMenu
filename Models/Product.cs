using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        public string ProductName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, 10000, ErrorMessage = "Fiyat 0.01 ile 10000 arasında olmalıdır.")]
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        // Navigation properties
        public List<ProductProperty>? ProductProperties { get; set; }
    }
    public class ProductUS: Product
    {
        public decimal? USPrice { get; set; }
    }
} 