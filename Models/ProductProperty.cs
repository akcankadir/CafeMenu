namespace CafeMenu.Models
{
    public class ProductProperty
    {
        public int ProductPropertyId { get; set; }
        public int ProductId { get; set; }
        public int PropertyId { get; set; }
        
        // Navigation properties
        public string? ProductName { get; set; }
        public string? PropertyKey { get; set; }
        public string? PropertyValue { get; set; }
    }
} 