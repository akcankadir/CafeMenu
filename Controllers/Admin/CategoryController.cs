using CafeMenu.Models;
using CafeMenu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CafeMenu.Controllers.Admin
{
    [Authorize]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı ID'sini al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    category.CreatedUserId = userId;
                }
                else
                {
                    category.CreatedUserId = 1; // Varsayılan admin kullanıcısı
                }

                await _categoryService.CreateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropdown();
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await PopulateCategoriesDropdown(category.CategoryId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropdown(category.CategoryId);
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesDropdown(int? excludeCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            // Eğer bir kategori düzenleniyorsa, kendisini ve alt kategorilerini listeden çıkar
            if (excludeCategoryId.HasValue)
            {
                categories = categories.Where(c => c.CategoryId != excludeCategoryId.Value).ToList();
            }

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
        }
    }
} 