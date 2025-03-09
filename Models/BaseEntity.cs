using System;

namespace CafeMenu.Models
{
    public abstract class BaseEntity
    {
        public int TenantId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatorUserId { get; set; }
    }
} 