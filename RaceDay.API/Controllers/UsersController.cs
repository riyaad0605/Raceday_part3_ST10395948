using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manage user profile including profile picture upload.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;

        public UsersController(RaceDayDbContext context, IBlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        /// <summary>Get the current user's profile.</summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new UserResponseDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImageUrl
            });
        }

        /// <summary>Update the current user's name.</summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImageUrl
            });
        }

        /// <summary>Upload a profile picture. File is stored in Azure Blob Storage.</summary>
        [HttpPost("profile/image")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPEG, PNG, GIF, and WebP images are allowed." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                await _blobService.DeleteFileAsync(user.ProfileImageUrl, "profile-images");

            using var stream = file.OpenReadStream();
            var imageUrl = await _blobService.UploadFileAsync(stream, file.FileName, file.ContentType, "profile-images");

            user.ProfileImageUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl });
        }
    }
}
