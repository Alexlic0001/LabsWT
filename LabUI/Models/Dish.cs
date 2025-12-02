using System.ComponentModel.DataAnnotations;

namespace LabUI.Models
{
    public class Dish
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? Image { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}