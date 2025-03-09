using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        public string[] Roles { get; set; } = Array.Empty<string>();
    }
} 