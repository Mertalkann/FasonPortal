using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FasonPortal.Models;
using FasonPortal.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FasonPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Roles = await _userManager.GetRolesAsync(user),
                    FabrikaAd = user.FabrikaId.HasValue ? _context.Fabrikalar.FirstOrDefault(f => f.Id == user.FabrikaId.Value)?.Ad : null,
                    AtolyeAd = user.AtolyeId.HasValue ? _context.Atolyeler.FirstOrDefault(a => a.Id == user.AtolyeId.Value)?.Ad : null
                };
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }
        // Kullanıcı oluşturma sayfası
        public IActionResult Create()
        {
            // Rolleri alıyoruz
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

            // Boş fabrika/atölye listesi gönderiyoruz, rol seçimine göre dolacak
            ViewBag.FabrikaAtolyeList = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.FullName,
                    FabrikaId = model.Role == "FabrikaYetkilisi" ? model.SelectedFabrikaAtolye : null,
                    AtolyeId = model.Role == "AtolyeYetkilisi" ? model.SelectedFabrikaAtolye : null
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            ViewBag.FabrikaAtolyeList = model.Role == "FabrikaYetkilisi"
                ? new SelectList(_context.Fabrikalar.ToList(), "Id", "Ad", model.SelectedFabrikaAtolye)
                : model.Role == "AtolyeYetkilisi"
                ? new SelectList(_context.Atolyeler.ToList(), "Id", "Ad", model.SelectedFabrikaAtolye)
                : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

            return View(model);
        }

        // Kullanıcı düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Role = role,
                FabrikaId = user.FabrikaId,
                AtolyeId = user.AtolyeId,
                SelectedFabrikaAtolye = user.FabrikaId ?? user.AtolyeId
            };

            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name", role);
            ViewBag.FabrikaAtolyeList = role == "FabrikaYetkilisi"
                ? new SelectList(_context.Fabrikalar.ToList(), "Id", "Ad", model.FabrikaId)
                : role == "AtolyeYetkilisi"
                ? new SelectList(_context.Atolyeler.ToList(), "Id", "Ad", model.AtolyeId)
                : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.FullName = model.FullName;
                user.FabrikaId = model.Role == "FabrikaYetkilisi" ? model.SelectedFabrikaAtolye : null;
                user.AtolyeId = model.Role == "AtolyeYetkilisi" ? model.SelectedFabrikaAtolye : null;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name", model.Role);
            ViewBag.FabrikaAtolyeList = model.Role == "FabrikaYetkilisi"
                ? new SelectList(_context.Fabrikalar.ToList(), "Id", "Ad", model.SelectedFabrikaAtolye)
                : model.Role == "AtolyeYetkilisi"
                ? new SelectList(_context.Atolyeler.ToList(), "Id", "Ad", model.SelectedFabrikaAtolye)
                : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

            return View(model);
        }

        public JsonResult GetFabrikaOrAtolye(string role)
        {
            if (role == "FabrikaYetkilisi")
            {
                var fabrikalar = _context.Fabrikalar.Select(f => new { id = f.Id, ad = f.Ad }).ToList();
                return Json(fabrikalar);
            }
            else if (role == "AtolyeYetkilisi")
            {
                var atolyeler = _context.Atolyeler.Select(a => new { id = a.Id, ad = a.Ad }).ToList();
                return Json(atolyeler);
            }
            return Json(Enumerable.Empty<SelectListItem>());
        }





        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
        
    }
}
