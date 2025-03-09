using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CafeMenu.Models;
using CafeMenu.Models.ViewModels;
using CafeMenu.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CafeMenu.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.ValidateUserAsync(model.Username, model.Password);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.GivenName, user.Name),
                        new Claim(ClaimTypes.Surname, user.Surname),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("UserId", user.UserId.ToString())
                    };

                    // Kullanıcı rollerini ekle
                    foreach (var userRole in user.UserRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var result = await _userService.UpdatePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (result)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Mevcut şifreniz hatalı.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var user = await _userService.GetUserByIdAsync(userId);

            var model = new ProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.Name,
                LastName = user.Surname,
                Roles = user.UserRoles?.Select(r => r.Role.Name).ToArray() ?? new string[] { }
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.IsEmailUniqueAsync(model.Email, model.UserId))
                {
                    var user = new User
                    {
                        UserId = model.UserId,
                        Email = model.Email,
                        Name = model.FirstName,
                        Surname = model.LastName
                    };

                    await _userService.UpdateProfileAsync(user);
                    TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
            }

            var currentUser = await _userService.GetUserByIdAsync(model.UserId);
            model.Roles = currentUser.UserRoles?.Select(r => r.Role.Name).ToArray() ?? new string[] { };

            return View(model);
        }
    }
} 