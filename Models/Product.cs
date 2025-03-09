using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CafeMenu.Models
{
    public class Product : BaseEntity
    {
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        // Ürün özellikleri ilişkisi
        public virtual ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
    }
} 