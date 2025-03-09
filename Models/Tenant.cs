using System;
using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Domain { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
} 