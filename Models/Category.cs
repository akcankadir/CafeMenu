using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeMenu.Models
{
    public class Category : BaseEntity
    {
        [Key]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;
        
        public int? ParentCategoryId { get; set; }
        
        [ForeignKey("ParentCategoryId")]
        public virtual Category? ParentCategory { get; set; }
        
        // Multi-tenancy için
        public int TenantId { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        // Alt kategoriler ve ürünler ilişkisi
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 