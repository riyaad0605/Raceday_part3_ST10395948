using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty; // Run, Walk, Cycle

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Published, Completed, Cancelled

        public string? BannerImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
