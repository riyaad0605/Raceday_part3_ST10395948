using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApiService _api;

        public ResultsController(ApiService api) { _api = api; }

        // Participant: My Results
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var results = await _api.GetMyResultsAsync();
            return View(results);
        }

        // Organiser: view/manage results for an event
        public async Task<IActionResult> EventResults(int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var results = await _api.GetEventResultsAsync(eventId);
            var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
            var ev = await _api.GetEventAsync(eventId);

            ViewBag.EventName = ev?.Name ?? "Event";
            ViewBag.EventId = eventId;
            ViewBag.Enrolments = enrolments.Where(e => e.Status == "Confirmed").ToList();
            ViewBag.ExistingEnrolmentIds = results.Select(r => r.EnrolmentId).ToHashSet();

            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> Capture(CaptureResultViewModel model, int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var (success, message, _) = await _api.CaptureResultAsync(model);
            TempData[success ? "Success" : "Error"] = success ? "Result captured!" : message;
            return RedirectToAction("EventResults", new { eventId });
        }
    }
}
