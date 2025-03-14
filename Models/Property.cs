using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models
{
    public class Property
    {
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Özellik adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Özellik adı en fazla 100 karakter olabilir.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Özellik değeri zorunludur.")]
        [StringLength(255, ErrorMessage = "Özellik değeri en fazla 255 karakter olabilir.")]
        public string Value { get; set; } = string.Empty;
    }
} 