using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly ApiService _api;

        public EnrolmentsController(ApiService api) { _api = api; }

        // Participant: My Events (enrolled events)
        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var enrolments = await _api.GetMyEnrolmentsAsync();
            return View(enrolments);
        }

        // Organiser: view enrolments for a specific event
        public async Task<IActionResult> EventEnrolments(int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
            var ev = await _api.GetEventAsync(eventId);
            ViewBag.EventName = ev?.Name ?? "Event";
            ViewBag.EventId = eventId;
            return View(enrolments);
        }
    }
}
