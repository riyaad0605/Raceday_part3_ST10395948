using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api) { _api = api; }

        // Participant home — browse/filter published events
        public async Task<IActionResult> Index(string? search, string? eventType)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Auth");
            if (role == "Organiser") return RedirectToAction("Dashboard");

            var events = await _api.GetEventsAsync();

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(eventType))
                events = events.Where(e => e.EventType == eventType).ToList();

            ViewBag.Search = search;
            ViewBag.EventType = eventType;
            return View(events);
        }

        // Organiser dashboard
        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Auth");
            if (role != "Organiser") return RedirectToAction("Index");

            var events = await _api.GetEventsAsync();
            var dashboard = new DashboardViewModel
            {
                TotalEvents = events.Count,
                PublishedEvents = events.Count(e => e.Status == "Published"),
                TotalEnrolments = events.Sum(e => e.TotalEnrolments),
                UpcomingEvents = events.Where(e => e.EventDate > DateTime.UtcNow)
                    .OrderBy(e => e.EventDate).Take(5).ToList()
            };

            return View(dashboard);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
