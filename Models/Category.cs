using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }

        // Navigation properties
        public List<Category>? SubCategories { get; set; }
        public List<Product>? Products { get; set; }
    }
} 