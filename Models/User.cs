using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeMenu.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(255, ErrorMessage = "Şifre en fazla 255 karakter olabilir.")]
        public string Password { get; set; } = string.Empty;
        
        // Veritabanında saklanmayan hesaplanmış özellikler
        [NotMapped]
        public bool IsAdmin => UserName?.ToLower() == "admin";
        
        [NotMapped]
        public string FullName => $"{Name} ({UserName})";
        
        [NotMapped]
        public string? Email => $"{UserName.ToLower()}@cafemenu.com";
    }
} 