using Microsoft.AspNetCore.Mvc;
using FasonPortal.Data;
using FasonPortal.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class IsTipiManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public IsTipiManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: İş Tipleri Index
    public async Task<IActionResult> Index()
    {
        var isTipleri = await _context.IsTipleri
            .Include(i => i.AtolyeIsler)
            .Include(i => i.IsEmirleri)
            .ToListAsync();

        return View(isTipleri);
    }

    // GET: İş Tipi Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: İş Tipi Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IsTipi model)
    {
        ModelState.Remove("IsEmirleri");
        ModelState.Remove("AtolyeIsler");

        if (ModelState.IsValid)
        {
            _context.IsTipleri.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: İş Tipi Edit
    public IActionResult Edit(int id)
    {
        var isTipi = _context.IsTipleri.Find(id);
        if (isTipi == null)
        {
            return NotFound();
        }
        return View(isTipi);
    }

    // POST: İş Tipi Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(IsTipi model)
    {
        ModelState.Remove("IsEmirleri");
        ModelState.Remove("AtolyeIsler");

        if (ModelState.IsValid)
        {
            _context.IsTipleri.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // POST: İş Tipi Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var isTipi = _context.IsTipleri.Find(id);
        if (isTipi == null)
        {
            return NotFound();
        }

        _context.IsTipleri.Remove(isTipi);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
