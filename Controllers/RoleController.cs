using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeMenu.Models;
using CafeMenu.Services;
using System.Linq;
using System;

namespace CafeMenu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles.OrderBy(r => r.Name));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                if (await _roleService.IsRoleNameUniqueAsync(role.Name))
                {
                    role.TenantId = GetTenantId();
                    var createdRole = await _roleService.CreateRoleAsync(role);
                    TempData["SuccessMessage"] = "Rol başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Name", "Bu rol adı zaten kullanılıyor.");
                }
            }
            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                if (await _roleService.IsRoleNameUniqueAsync(role.Name, role.RoleId))
                {
                    role.TenantId = GetTenantId();
                    var updatedRole = await _roleService.UpdateRoleAsync(role);
                    TempData["SuccessMessage"] = "Rol başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Name", "Bu rol adı zaten kullanılıyor.");
                }
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteRoleAsync(id);
            TempData["SuccessMessage"] = "Rol başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // Yardımcı metot
        private int GetTenantId()
        {
            return HttpContext.Items.ContainsKey("TenantId") ? (int)HttpContext.Items["TenantId"] : 0;
        }
    }
} 