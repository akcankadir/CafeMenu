using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeMenu.Models
{
    public class Property : BaseEntity
    {
        [Key]
        public int PropertyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        // Ürün özellikleri ilişkisi
        public virtual ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
    }
} 