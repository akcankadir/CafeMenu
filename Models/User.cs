using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeMenu.Models
{
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Surname { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string HashPassword { get; set; } = string.Empty;
        
        [Required]
        public Guid SaltPassword { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        // Kullanıcı rolleri
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        [NotMapped]
        public string FullName => $"{Name} {Surname}";
    }
} 