using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        public int FinishTimeSeconds { get; set; }

        public int Position { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }
    }
}
