using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _api;

        public EventsController(ApiService api) { _api = api; }

        // Organiser: list all events
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var events = await _api.GetEventsAsync();
            return View(events);
        }

        // Participant: event detail with categories and enrol buttons
        public async Task<IActionResult> Details(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Auth");

            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();

            var categories = await _api.GetEventCategoriesAsync(id);
            var myEnrolments = role == "Participant" ? await _api.GetMyEnrolmentsAsync() : new();

            var vm = new EventDetailViewModel
            {
                Event = ev,
                Categories = categories,
                MyEnrolments = myEnrolments.Where(e => categories.Any(c => c.EventCategoryId == e.EventCategoryId)).ToList()
            };

            return View(vm);
        }

        // Organiser: create event form
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");
            return View(new CreateEventViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model, IFormFile? bannerImage)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var (success, message, ev) = await _api.CreateEventAsync(model);
            if (!success)
            {
                ViewBag.Error = message;
                return View(model);
            }

            // Upload banner if provided
            if (bannerImage != null && bannerImage.Length > 0)
            {
                using var stream = bannerImage.OpenReadStream();
                await _api.UploadBannerAsync(ev!.EventId, stream, bannerImage.FileName, bannerImage.ContentType);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        // Organiser: edit event
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var ev = await _api.GetEventAsync(id);
            if (ev == null) return NotFound();

            var model = new CreateEventViewModel
            {
                Name = ev.Name, Description = ev.Description, EventDate = ev.EventDate,
                Location = ev.Location, EventType = ev.EventType, Status = ev.Status
            };

            ViewBag.EventId = id;
            ViewBag.BannerImageUrl = ev.BannerImageUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model, IFormFile? bannerImage)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var (success, message, _) = await _api.UpdateEventAsync(id, model);
            if (!success)
            {
                ViewBag.Error = message;
                ViewBag.EventId = id;
                return View(model);
            }

            if (bannerImage != null && bannerImage.Length > 0)
            {
                using var stream = bannerImage.OpenReadStream();
                await _api.UploadBannerAsync(id, stream, bannerImage.FileName, bannerImage.ContentType);
            }

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var (success, message) = await _api.DeleteEventAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Event deleted." : message;
            return RedirectToAction("Index");
        }

        // Participant: enrol in a category
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventCategoryId, int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var (success, message, _) = await _api.EnrolAsync(eventCategoryId);
            TempData[success ? "Success" : "Error"] = success ? "Enrolled successfully!" : message;
            return RedirectToAction("Details", new { id = eventId });
        }

        // Participant: withdraw
        [HttpPost]
        public async Task<IActionResult> Withdraw(int enrolmentId, int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var (success, message) = await _api.WithdrawAsync(enrolmentId);
            TempData[success ? "Success" : "Error"] = success ? "Withdrawn successfully." : message;
            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
