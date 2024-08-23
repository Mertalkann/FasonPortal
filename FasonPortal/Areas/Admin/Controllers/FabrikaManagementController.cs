using DinkToPdf.Contracts;
using DinkToPdf;
using FasonPortal.Data;
using FasonPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FasonPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FabrikaManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;

        public FabrikaManagementController(ApplicationDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        // Index Metodu
        public IActionResult Index()
        {
            var fabrikalar = _context.Fabrikalar
                .Include(f => f.Users)
                .Include(f => f.IsEmirleri)
                    .ThenInclude(ie => ie.IsTipi)
                .Include(f => f.IsEmirleri)
                    .ThenInclude(ie => ie.Atolye)
                .ToList();

            var fabrikalarViewModel = fabrikalar.Select(fabrika => new
            {
                Fabrika = fabrika,
                KullaniciSayisi = fabrika.Users.Count(),
                ToplamSiparis = fabrika.IsEmirleri.Count(),
                ToplamSiparisTutari = fabrika.IsEmirleri.Where(e => e.Durum != "İptal Edildi").Sum(e => e.BirimFiyat * e.Adet),
                IptalSiparisSayisi = fabrika.IsEmirleri.Count(e => e.Durum == "İptal Edildi"),
                IptalSiparisTutari = fabrika.IsEmirleri.Where(e => e.Durum == "İptal Edildi").Sum(e => e.BirimFiyat * e.Adet),
                AtolyeBazliGruplar = fabrika.IsEmirleri
                    .Where(e => e.Durum != "İptal Edildi")
                    .GroupBy(e => e.Atolye.Ad)
                    .Select(g => new
                    {
                        AtolyeAd = g.Key,
                        IsTipiGruplar = g.GroupBy(ie => ie.IsTipi.Ad)
                            .Select(ig => new
                            {
                                IsTipiAd = ig.Key,
                                ToplamAdet = ig.Sum(ie => ie.Adet),
                                ToplamTutar = ig.Sum(ie => ie.BirimFiyat * ie.Adet)
                            }).ToList(),
                        ToplamAdet = g.Sum(ie => ie.Adet),
                        ToplamTutar = g.Sum(ie => ie.BirimFiyat * ie.Adet)
                    }).ToList(),
                IsTipiBazliGruplar = fabrika.IsEmirleri
                    .Where(e => e.Durum != "İptal Edildi")
                    .GroupBy(e => e.IsTipi.Ad)
                    .Select(g => new
                    {
                        IsTipiAd = g.Key,
                        AtolyeGruplar = g.GroupBy(ie => ie.Atolye.Ad)
                            .Select(ag => new
                            {
                                AtolyeAd = ag.Key,
                                ToplamAdet = ag.Sum(ie => ie.Adet),
                                ToplamTutar = ag.Sum(ie => ie.BirimFiyat * ie.Adet)
                            }).ToList(),
                        ToplamAdet = g.Sum(ie => ie.Adet),
                        ToplamTutar = g.Sum(ie => ie.BirimFiyat * ie.Adet)
                    }).ToList(),
                IptalEdilenIsEmirleri = fabrika.IsEmirleri
                    .Where(e => e.Durum == "İptal Edildi")
                    .Select(e => new
                    {
                        e.Adet,
                        e.BirimFiyat,
                        AtolyeAd = e.Atolye.Ad,
                        IsTipiAd = e.IsTipi.Ad,
                        ToplamTutar = e.Adet * e.BirimFiyat
                    }).ToList(),
            }).ToList();

            return View(fabrikalarViewModel);
        }










        // Yeni fabrika oluşturma
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fabrika model)
        {
            ModelState.Remove("Users");
            ModelState.Remove("IsEmirleri");
            if (ModelState.IsValid)
            {
                _context.Fabrikalar.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Fabrika düzenleme
        public IActionResult Edit(int id)
        {
            var fabrika = _context.Fabrikalar.Find(id);
            if (fabrika == null)
            {
                return NotFound();
            }
            return View(fabrika);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fabrika model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            ModelState.Remove("Users");
            ModelState.Remove("IsEmirleri");
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Fabrika silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var fabrika = await _context.Fabrikalar.FindAsync(id);
            if (fabrika != null)
            {
                _context.Fabrikalar.Remove(fabrika);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Fabrika sipariş raporu alma
        public async Task<IActionResult> RaporAl(int fabrikaId)
        {
            var fabrika = await _context.Fabrikalar
                .Include(f => f.IsEmirleri)
                .ThenInclude(e => e.IsTipi)
                .Include(f => f.IsEmirleri)
                .ThenInclude(e => e.Atolye)
                .FirstOrDefaultAsync(f => f.Id == fabrikaId);

            if (fabrika == null)
            {
                return NotFound();
            }

            var aktifIsEmirleri = fabrika.IsEmirleri
                .Where(e => e.Durum != "İptal Edildi")
                .OrderByDescending(e => e.OlusturulmaTarihi)
                .ToList();

            var tumIsEmirleri = fabrika.IsEmirleri
                .OrderByDescending(e => e.OlusturulmaTarihi)
                .ToList();

            var model = new FabrikaRaporViewModel
            {
                FabrikaAd = fabrika.Ad,
                IsEmirleri = tumIsEmirleri.Select(e => new IsEmriViewModel
                {
                    Id = e.Id,
                    IsTipiAd = e.IsTipi?.Ad,
                    AtolyeAd = e.Atolye?.Ad,
                    Adet = e.Adet,
                    BirimFiyat = e.BirimFiyat,
                    Aciklama = e.Aciklama,
                    Durum = e.Durum,
                    OlusturulmaTarihi = e.OlusturulmaTarihi
                }).ToList()
            };

            ViewBag.AtolyeBazliGruplar = aktifIsEmirleri
                .GroupBy(e => e.Atolye.Ad)
                .Select(g => new
                {
                    AtolyeAd = g.Key,
                    IsTipiGruplar = g.GroupBy(ie => ie.IsTipi.Ad)
                        .Select(ig => new
                        {
                            IsTipiAd = ig.Key,
                            ToplamAdet = ig.Sum(ie => ie.Adet),
                            ToplamTutar = ig.Sum(ie => ie.BirimFiyat * ie.Adet)
                        }).ToList(),
                    ToplamAdet = g.Sum(ie => ie.Adet),
                    ToplamTutar = g.Sum(ie => ie.BirimFiyat * ie.Adet)
                }).ToList();

            ViewBag.IsTipiBazliGruplar = aktifIsEmirleri
                .GroupBy(e => e.IsTipi.Ad)
                .Select(g => new
                {
                    IsTipiAd = g.Key,
                    AtolyeGruplar = g.GroupBy(ie => ie.Atolye.Ad)
                        .Select(ag => new
                        {
                            AtolyeAd = ag.Key,
                            ToplamAdet = ag.Sum(ie => ie.Adet),
                            ToplamTutar = ag.Sum(ie => ie.BirimFiyat * ie.Adet)
                        }).ToList(),
                    ToplamAdet = g.Sum(ie => ie.Adet),
                    ToplamTutar = g.Sum(ie => ie.BirimFiyat * ie.Adet)
                }).ToList();

            var htmlContent = await this.RenderViewAsync("RaporTemplate", model, true);

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
        },
                Objects = {
            new ObjectSettings() {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdfBytes = _converter.Convert(pdf);
            return File(pdfBytes, "application/pdf", "FabrikaSiparisRaporu.pdf");
        }











    }

}

