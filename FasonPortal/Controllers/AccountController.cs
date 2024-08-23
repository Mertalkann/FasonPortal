using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FasonPortal.Models;
using System.Threading.Tasks;
using FasonPortal.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace FasonPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
                    }
                    else if (roles.Contains("FabrikaYetkilisi"))
                    {
                        return RedirectToAction("Index", "Fabrika");
                    }
                    else if (roles.Contains("AtolyeYetkilisi"))
                    {
                        return RedirectToAction("Index", "Atolye");
                    }
                }
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}