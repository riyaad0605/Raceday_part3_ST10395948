using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventDTO
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";
    }

    public class EventResponseDTO
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalEnrolments { get; set; }
    }

    public class CategoryDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }
    }

    public class CategoryResponseDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
    }

    public class EventCategoryDTO
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int MaxParticipants { get; set; } = 500;
    }

    public class EventCategoryResponseDTO
    {
        public int EventCategoryId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentEnrolments { get; set; }
    }

    public class EnrolmentDTO
    {
        [Required]
        public int EventCategoryId { get; set; }
    }

    public class EnrolmentResponseDTO
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventCategoryId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ResultDTO
    {
        [Required]
        public int EnrolmentId { get; set; }

        public int FinishTimeSeconds { get; set; }

        public int Position { get; set; }
    }

    public class ResultResponseDTO
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int FinishTimeSeconds { get; set; }
        public int Position { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}
