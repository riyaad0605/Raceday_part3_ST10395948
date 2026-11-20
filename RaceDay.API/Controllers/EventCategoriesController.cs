using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>Link categories to events and view event-category relationships.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventCategoriesController(RaceDayDbContext context) { _context = context; }

        /// <summary>Get all categories for a specific event with enrolment counts.</summary>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var eventCategories = await _context.EventCategories
                .Include(ec => ec.Event).Include(ec => ec.Category).Include(ec => ec.Enrolments)
                .Where(ec => ec.EventId == eventId)
                .Select(ec => new EventCategoryResponseDTO
                {
                    EventCategoryId = ec.EventCategoryId, EventId = ec.EventId, EventName = ec.Event!.Name,
                    CategoryId = ec.CategoryId, CategoryName = ec.Category!.Name, DistanceKm = ec.Category.DistanceKm,
                    MaxParticipants = ec.MaxParticipants,
                    CurrentEnrolments = ec.Enrolments.Count(en => en.Status == "Confirmed")
                }).ToListAsync();

            return Ok(eventCategories);
        }

        /// <summary>Link a category to an event. Owner Organiser only.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCategoryDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can link categories." });

            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId.Value) return StatusCode(403, new { message = "You can only manage your own events." });

            var cat = await _context.Categories.FindAsync(dto.CategoryId);
            if (cat == null) return NotFound(new { message = "Category not found." });

            var ec = new EventCategory { EventId = dto.EventId, CategoryId = dto.CategoryId, MaxParticipants = dto.MaxParticipants };
            _context.EventCategories.Add(ec);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEvent), new { eventId = ec.EventId }, new EventCategoryResponseDTO
            {
                EventCategoryId = ec.EventCategoryId, EventId = ec.EventId, EventName = ev.Name,
                CategoryId = ec.CategoryId, CategoryName = cat.Name, DistanceKm = cat.DistanceKm,
                MaxParticipants = ec.MaxParticipants, CurrentEnrolments = 0
            });
        }

        /// <summary>Remove a category link from an event. Owner Organiser only.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can unlink categories." });

            var ec = await _context.EventCategories.Include(x => x.Event).FirstOrDefaultAsync(x => x.EventCategoryId == id);
            if (ec == null) return NotFound(new { message = "Event category not found." });
            if (ec.Event!.OrganiserId != userId.Value) return StatusCode(403, new { message = "You can only manage your own events." });

            _context.EventCategories.Remove(ec);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
