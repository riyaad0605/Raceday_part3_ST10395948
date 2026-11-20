using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;

        public AuthController(ApiService api) { _api = api; }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var (success, message, user) = await _api.LoginAsync(model);
            if (!success)
            {
                ViewBag.Error = message;
                return View(model);
            }

            // Store user info in MVC session
            HttpContext.Session.SetInt32("UserId", user!.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserRole", user.Role);
            if (user.ProfileImageUrl != null)
                HttpContext.Session.SetString("ProfileImageUrl", user.ProfileImageUrl);

            return user.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Home")
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var (success, message, user) = await _api.RegisterAsync(model);
            if (!success)
            {
                ViewBag.Error = message;
                return View(model);
            }

            TempData["Success"] = "Account created! Please log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
