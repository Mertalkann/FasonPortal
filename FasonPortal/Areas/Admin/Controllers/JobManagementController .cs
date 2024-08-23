using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FasonPortal.Data;
using FasonPortal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class JobManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Siparişler sayfası (Admin tüm siparişleri görür)
    public async Task<IActionResult> Index()
    {
        var isEmirleri = await _context.IsEmirleri
            .Include(e => e.IsTipi)
            .Include(e => e.Atolye)
            .Include(e => e.Fabrika)
            .OrderByDescending(e => e.OlusturulmaTarihi)
            .ToListAsync();

        return View(isEmirleri);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        // İlgili iş emrini bul
        var isEmri = await _context.IsEmirleri.FindAsync(id);

        if (isEmri != null)
        {
            // Siparişin durumunu "İptal Edildi" olarak değiştir
            isEmri.Durum = "İptal Edildi";
            _context.Update(isEmri);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return NotFound();
    }

    // Sipariş düzenleme sayfası
    public IActionResult Edit(int id)
    {
        var isEmri = _context.IsEmirleri
            .Include(e => e.IsTipi)
            .Include(e => e.Atolye)
            .Include(e => e.Fabrika)
            .FirstOrDefault(e => e.Id == id);

        if (isEmri == null)
        {
            return NotFound();
        }

        var model = new IsEmriViewModel
        {
            Id = isEmri.Id,
            IsTipiId = isEmri.IsTipiId,
            AtolyeId = isEmri.AtolyeId,
            FabrikaId = isEmri.FabrikaId,  // Burada FabrikaId ekleniyor
            Adet = isEmri.Adet,
            BirimFiyat = isEmri.BirimFiyat,
            Aciklama = isEmri.Aciklama,
            Durum = isEmri.Durum
        };

        ViewBag.IsTipleri = new SelectList(_context.IsTipleri, "Id", "Ad", model.IsTipiId);
        ViewBag.Atolyeler = new SelectList(_context.AtolyeIsler
            .Where(ai => ai.IsTipiId == model.IsTipiId)
            .Select(ai => ai.Atolye)
            .ToList(), "Id", "Ad", model.AtolyeId);
        ViewBag.Fabrikalar = new SelectList(_context.Fabrikalar, "Id", "Ad", isEmri.FabrikaId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(IsEmriViewModel model)
    {
        // AtolyeAd, IsTipiAd, ve FabrikaAd alanlarını ModelState'den kaldırıyoruz
        ModelState.Remove("AtolyeAd");
        ModelState.Remove("IsTipiAd");
        ModelState.Remove("FabrikaAd");

        if (ModelState.IsValid)
        {
            var isEmri = await _context.IsEmirleri.FindAsync(model.Id);
            if (isEmri == null)
            {
                return NotFound();
            }

            isEmri.IsTipiId = model.IsTipiId;
            isEmri.AtolyeId = model.AtolyeId;
            isEmri.FabrikaId = model.FabrikaId;
            isEmri.Adet = model.Adet;
            isEmri.Aciklama = model.Aciklama;
            isEmri.BirimFiyat = model.BirimFiyat;
            isEmri.Durum = model.Durum;

            _context.Update(isEmri);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hatalı durumda iş tiplerini ve atölyeleri tekrar gönderiyoruz
        ViewBag.IsTipleri = new SelectList(_context.IsTipleri, "Id", "Ad", model.IsTipiId);
        ViewBag.Atolyeler = new SelectList(_context.AtolyeIsler
            .Where(ai => ai.IsTipiId == model.IsTipiId)
            .Select(ai => ai.Atolye)
            .ToList(), "Id", "Ad", model.AtolyeId);
        ViewBag.Fabrikalar = new SelectList(_context.Fabrikalar, "Id", "Ad", model.FabrikaId);

        return View(model);
    }

    // AJAX ile İş Tipine göre atölyeleri getirme
    public JsonResult GetAtolyelerByIsTipi(int isTipiId)
    {
        var atolyeler = _context.AtolyeIsler
            .Where(ai => ai.IsTipiId == isTipiId)
            .Select(ai => new { ai.Atolye.Id, ai.Atolye.Ad })
            .ToList();

        return Json(atolyeler);
    }

    // AJAX ile Atölye ve İş Tipine göre birim fiyatı getirme
    public JsonResult GetBirimFiyat(int atolyeId, int isTipiId)
    {
        var birimFiyat = _context.AtolyeIsler
            .Where(ai => ai.AtolyeId == atolyeId && ai.IsTipiId == isTipiId)
            .Select(ai => ai.BirimFiyat)
            .FirstOrDefault();

        return Json(new { birimFiyat });
    }
}
