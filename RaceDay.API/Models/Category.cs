using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        // Navigation
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
