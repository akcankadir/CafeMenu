using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeMenu.Models
{
    public class ProductProperty : BaseEntity
    {
        [Key]
        public int ProductPropertyId { get; set; }
        
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        
        public int PropertyId { get; set; }
        
        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
} 