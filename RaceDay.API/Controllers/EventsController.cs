using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Full CRUD for race day events. Organisers manage their own events; Participants see published ones.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;

        public EventsController(RaceDayDbContext context, IBlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        /// <summary>Get all events. Organisers see all; Participants see only Published.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");

            var query = _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Enrolments)
                .AsQueryable();

            if (role == "Participant")
                query = query.Where(e => e.Status == "Published");

            var events = await query.Select(e => new EventResponseDTO
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                EventType = e.EventType,
                Status = e.Status,
                BannerImageUrl = e.BannerImageUrl,
                CreatedAt = e.CreatedAt,
                TotalEnrolments = e.EventCategories.SelectMany(ec => ec.Enrolments).Count(en => en.Status == "Confirmed")
            }).ToListAsync();

            return Ok(events);
        }

        /// <summary>Get a single event by ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var e = await _context.Events
                .Include(ev => ev.Organiser)
                .Include(ev => ev.EventCategories)
                    .ThenInclude(ec => ec.Enrolments)
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (e == null) return NotFound(new { message = "Event not found." });

            return Ok(new EventResponseDTO
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                EventType = e.EventType,
                Status = e.Status,
                BannerImageUrl = e.BannerImageUrl,
                CreatedAt = e.CreatedAt,
                TotalEnrolments = e.EventCategories.SelectMany(ec => ec.Enrolments).Count(en => en.Status == "Confirmed")
            });
        }

        /// <summary>Create a new event. Organisers only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] EventDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can create events." });

            var ev = new Event
            {
                OrganiserId = userId.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                EventType = dto.EventType,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId.Value);

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new EventResponseDTO
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = user!.FirstName + " " + user.LastName,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Status = ev.Status,
                BannerImageUrl = ev.BannerImageUrl,
                CreatedAt = ev.CreatedAt,
                TotalEnrolments = 0
            });
        }

        /// <summary>Update an event. Owner Organiser only.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, [FromBody] EventDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can update events." });

            var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only update your own events." });

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.EventType = dto.EventType;
            ev.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new EventResponseDTO
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = ev.Organiser!.FirstName + " " + ev.Organiser.LastName,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Status = ev.Status,
                BannerImageUrl = ev.BannerImageUrl,
                CreatedAt = ev.CreatedAt,
                TotalEnrolments = 0
            });
        }

        /// <summary>Delete an event. Owner Organiser only.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can delete events." });

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only delete your own events." });

            // Delete banner image from blob storage
            if (!string.IsNullOrEmpty(ev.BannerImageUrl))
                await _blobService.DeleteFileAsync(ev.BannerImageUrl, "event-banners");

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Upload a banner image for an event. Owner Organiser only.</summary>
        [HttpPost("{id}/banner")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can upload event images." });

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only upload images for your own events." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPEG, PNG, GIF, and WebP images are allowed." });

            // Delete old banner if exists
            if (!string.IsNullOrEmpty(ev.BannerImageUrl))
                await _blobService.DeleteFileAsync(ev.BannerImageUrl, "event-banners");

            using var stream = file.OpenReadStream();
            var imageUrl = await _blobService.UploadFileAsync(stream, file.FileName, file.ContentType, "event-banners");

            ev.BannerImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl });
        }
    }
}
