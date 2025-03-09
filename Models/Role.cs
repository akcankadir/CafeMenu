using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CafeMenu.Models
{
    public class Role : BaseEntity
    {
        [Key]
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsSystemRole { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
        
        // Kullanıcı rolleri
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        [NotMapped]
        public virtual ICollection<User> Users => UserRoles?.Select(ur => ur.User).ToList() ?? new List<User>();
    }
    
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        public int RoleId { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
} 