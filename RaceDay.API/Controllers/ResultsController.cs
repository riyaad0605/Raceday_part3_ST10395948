using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>Capture and view race results for completed events.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context) { _context = context; }

        /// <summary>Capture a result for a confirmed enrolment. Organisers only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResultResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> CaptureResult([FromBody] ResultDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can capture results." });

            var enrolment = await _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.EventCategory).ThenInclude(ec => ec!.Event)
                .Include(en => en.EventCategory).ThenInclude(ec => ec!.Category)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(en => en.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.EventCategory!.Event!.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only capture results for your own events." });
            if (enrolment.Result != null) return BadRequest(new { message = "A result already exists for this enrolment." });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSeconds = dto.FinishTimeSeconds,
                Position = dto.Position,
                CapturedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEvent), new { eventId = enrolment.EventCategory.EventId }, new ResultResponseDTO
            {
                ResultId = result.ResultId, EnrolmentId = result.EnrolmentId,
                ParticipantName = enrolment.Participant!.FirstName + " " + enrolment.Participant.LastName,
                EventName = enrolment.EventCategory.Event.Name,
                CategoryName = enrolment.EventCategory.Category!.Name,
                FinishTimeSeconds = result.FinishTimeSeconds, Position = result.Position, CapturedAt = result.CapturedAt
            });
        }

        /// <summary>Get all results for a specific event.</summary>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Event)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Category)
                .Where(r => r.Enrolment!.EventCategory!.EventId == eventId)
                .Select(r => new ResultResponseDTO
                {
                    ResultId = r.ResultId, EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.EventCategory!.Event!.Name,
                    CategoryName = r.Enrolment.EventCategory.Category!.Name,
                    FinishTimeSeconds = r.FinishTimeSeconds, Position = r.Position, CapturedAt = r.CapturedAt
                }).ToListAsync();

            return Ok(results);
        }

        /// <summary>Get all results for the current Participant.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Event)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Category)
                .Where(r => r.Enrolment!.ParticipantId == userId.Value)
                .Select(r => new ResultResponseDTO
                {
                    ResultId = r.ResultId, EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.EventCategory!.Event!.Name,
                    CategoryName = r.Enrolment.EventCategory.Category!.Name,
                    FinishTimeSeconds = r.FinishTimeSeconds, Position = r.Position, CapturedAt = r.CapturedAt
                }).ToListAsync();

            return Ok(results);
        }

        /// <summary>Update an existing result. Organisers only.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can update results." });

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Event)
                .Include(r => r.Enrolment).ThenInclude(en => en!.EventCategory).ThenInclude(ec => ec!.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound(new { message = "Result not found." });
            if (result.Enrolment!.EventCategory!.Event!.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only update results for your own events." });

            result.FinishTimeSeconds = dto.FinishTimeSeconds;
            result.Position = dto.Position;
            result.CapturedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ResultResponseDTO
            {
                ResultId = result.ResultId, EnrolmentId = result.EnrolmentId,
                ParticipantName = result.Enrolment.Participant!.FirstName + " " + result.Enrolment.Participant.LastName,
                EventName = result.Enrolment.EventCategory.Event.Name,
                CategoryName = result.Enrolment.EventCategory.Category!.Name,
                FinishTimeSeconds = result.FinishTimeSeconds, Position = result.Position, CapturedAt = result.CapturedAt
            });
        }
    }
}
