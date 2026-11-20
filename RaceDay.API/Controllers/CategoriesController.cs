using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>CRUD for race distance categories.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>Get all categories.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var categories = await _context.Categories.Select(c => new CategoryResponseDTO
            {
                CategoryId = c.CategoryId, Name = c.Name, Description = c.Description, DistanceKm = c.DistanceKm
            }).ToListAsync();

            return Ok(categories);
        }

        /// <summary>Get a single category.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var c = await _context.Categories.FindAsync(id);
            if (c == null) return NotFound(new { message = "Category not found." });

            return Ok(new CategoryResponseDTO { CategoryId = c.CategoryId, Name = c.Name, Description = c.Description, DistanceKm = c.DistanceKm });
        }

        /// <summary>Create a new category. Organisers only.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can create categories." });

            var cat = new Category { Name = dto.Name, Description = dto.Description, DistanceKm = dto.DistanceKm };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cat.CategoryId },
                new CategoryResponseDTO { CategoryId = cat.CategoryId, Name = cat.Name, Description = cat.Description, DistanceKm = cat.DistanceKm });
        }

        /// <summary>Update a category. Organisers only.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can update categories." });

            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound(new { message = "Category not found." });

            cat.Name = dto.Name;
            cat.Description = dto.Description;
            cat.DistanceKm = dto.DistanceKm;
            await _context.SaveChangesAsync();

            return Ok(new CategoryResponseDTO { CategoryId = cat.CategoryId, Name = cat.Name, Description = cat.Description, DistanceKm = cat.DistanceKm });
        }

        /// <summary>Delete a category. Organisers only.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Only Organisers can delete categories." });

            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
