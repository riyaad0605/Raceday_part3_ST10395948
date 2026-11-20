using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApiService _api;

        public ProfileController(ApiService api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Auth");

            var user = await _api.GetProfileAsync();
            if (user == null) return RedirectToAction("Login", "Auth");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            var (success, message, user) = await _api.UpdateProfileAsync(model);
            if (!success) { TempData["Error"] = message; return RedirectToAction("Index"); }

            HttpContext.Session.SetString("UserName", $"{user!.FirstName} {user.LastName}");
            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["Error"] = "Please select an image to upload.";
                return RedirectToAction("Index");
            }

            using var stream = profileImage.OpenReadStream();
            var (success, message) = await _api.UploadProfileImageAsync(stream, profileImage.FileName, profileImage.ContentType);
            TempData[success ? "Success" : "Error"] = success ? "Profile picture updated!" : message;
            return RedirectToAction("Index");
        }
    }
}
