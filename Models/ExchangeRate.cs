using System;
using System.ComponentModel.DataAnnotations;

namespace CafeMenu.Models
{
    public class ExchangeRate
    {
        [Key]
        public int ExchangeRateId { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrencyCode { get; set; }

        [Required]
        public decimal Rate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
} 