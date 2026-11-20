namespace RaceDay.MVC.Models
{
    // Auth
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Participant";
    }

    // User
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }

    public class UpdateProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    // Event
    public class EventViewModel
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

    public class CreateEventViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(30);
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = "Run";
        public string Status { get; set; } = "Draft";
    }

    // Category
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
    }

    public class CreateCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
    }

    // EventCategory
    public class EventCategoryViewModel
    {
        public int EventCategoryId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentEnrolments { get; set; }
    }

    public class LinkCategoryViewModel
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public int MaxParticipants { get; set; } = 50;
    }

    // Enrolment
    public class EnrolmentViewModel
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

    // Result
    public class ResultViewModel
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public double FinishTimeSeconds { get; set; }
        public int Position { get; set; }
        public DateTime CapturedAt { get; set; }

        public string FormattedTime
        {
            get
            {
                var ts = TimeSpan.FromSeconds(FinishTimeSeconds);
                return ts.Hours > 0
                    ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                    : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }
    }

    public class CaptureResultViewModel
    {
        public int EnrolmentId { get; set; }
        public double FinishTimeSeconds { get; set; }
        public int Position { get; set; }
    }

    // Dashboard
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<EventViewModel> UpcomingEvents { get; set; } = new();
    }

    // Event Detail (for Participant view)
    public class EventDetailViewModel
    {
        public EventViewModel Event { get; set; } = new();
        public List<EventCategoryViewModel> Categories { get; set; } = new();
        public List<EnrolmentViewModel> MyEnrolments { get; set; } = new();
    }
}
