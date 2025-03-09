using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafeMenu.Models;
using CafeMenu.Services;
using System.Linq;
using System.Collections.Generic;

namespace CafeMenu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _userService.GetAllRolesAsync();
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, int[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.IsUsernameUniqueAsync(user.Username))
                {
                    var createdUser = await _userService.CreateUserAsync(user, password);
                    if (selectedRoles != null && selectedRoles.Any())
                    {
                        await _userService.UpdateUserRolesAsync(createdUser.UserId, selectedRoles);
                    }

                    TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılıyor.");
            }

            ViewBag.Roles = await _userService.GetAllRolesAsync();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = await _userService.GetAllRolesAsync();
            var selectedRoles = user.UserRoles?.Select(ur => ur.RoleId).ToArray() ?? Array.Empty<int>();
            ViewBag.SelectedRoles = selectedRoles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, int[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(user);
                
                if (selectedRoles != null)
                {
                    await _userService.UpdateUserRolesAsync(user.UserId, selectedRoles);
                }

                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _userService.GetAllRolesAsync();
            ViewBag.SelectedRoles = selectedRoles ?? new int[] { };
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
} 