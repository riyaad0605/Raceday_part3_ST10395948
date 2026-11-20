using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApiService _api;

        public CategoriesController(ApiService api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");
            var categories = await _api.GetCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");
            return View(new CreateCategoryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            var (success, message, _) = await _api.CreateCategoryAsync(model);
            if (!success) { ViewBag.Error = message; return View(model); }
            TempData["Success"] = "Category created!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var cat = await _api.GetCategoryAsync(id);
            if (cat == null) return NotFound();

            ViewBag.CategoryId = id;
            return View(new CreateCategoryViewModel { Name = cat.Name, Description = cat.Description, DistanceKm = cat.DistanceKm });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateCategoryViewModel model)
        {
            var (success, message, _) = await _api.UpdateCategoryAsync(id, model);
            if (!success) { ViewBag.Error = message; ViewBag.CategoryId = id; return View(model); }
            TempData["Success"] = "Category updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _api.DeleteCategoryAsync(id);
            TempData[success ? "Success" : "Error"] = success ? "Category deleted." : message;
            return RedirectToAction("Index");
        }
    }
}
