
using FasonPortal.Data;
using FasonPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FasonPortal.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AtolyeManagementController : Controller
    {
        private readonly ApplicationDbContext _context;


        public AtolyeManagementController(ApplicationDbContext context)
        {
            _context = context;

        }

        // Index (Atölye listeleme)
        public IActionResult Index()
        {
            var atolyeler = _context.Atolyeler
                .Include(a => a.Users)
                .Include(a => a.IsEmirleri)
                    .ThenInclude(ie => ie.IsTipi)
                .Include(a => a.IsEmirleri)
                    .ThenInclude(ie => ie.Fabrika)
                .ToList();

            var atolyelerViewModel = atolyeler.Select(atolye => new
            {
                Atolye = atolye,
                AktifIsEmirleri = atolye.IsEmirleri.Where(e => e.Durum != "İptal Edildi").ToList(),
                IptalEdilenIsEmirleri = atolye.IsEmirleri.Where(e => e.Durum == "İptal Edildi").ToList(),
                ToplamIsTipi = atolye.IsEmirleri.Select(e => e.IsTipiId).Distinct().Count(),
                ToplamSiparis = atolye.IsEmirleri.Count,
                ToplamIptalSiparis = atolye.IsEmirleri.Count(e => e.Durum == "İptal Edildi"),
                ToplamKullanici = atolye.Users.Count,
                ToplamGelir = atolye.IsEmirleri.Where(e => e.Durum != "İptal Edildi").Sum(e => e.BirimFiyat * e.Adet),
                ToplamIptalTutar = atolye.IsEmirleri.Where(e => e.Durum == "İptal Edildi").Sum(e => e.BirimFiyat * e.Adet),
                FabrikaBazliGruplar = atolye.IsEmirleri.Where(e => e.Durum != "İptal Edildi")
                    .GroupBy(e => e.Fabrika.Ad)
                    .Select(g => new
                    {
                        FabrikaAd = g.Key,
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
                IsTipiBazliGruplar = atolye.IsEmirleri.Where(e => e.Durum != "İptal Edildi")
                    .GroupBy(e => e.IsTipi.Ad)
                    .Select(g => new
                    {
                        IsTipiAd = g.Key,
                        FabrikaGruplar = g.GroupBy(ie => ie.Fabrika.Ad)
                            .Select(fg => new
                            {
                                FabrikaAd = fg.Key,
                                ToplamAdet = fg.Sum(ie => ie.Adet),
                                ToplamTutar = fg.Sum(ie => ie.BirimFiyat * ie.Adet)
                            }).ToList(),
                        ToplamAdet = g.Sum(ie => ie.Adet),
                        ToplamTutar = g.Sum(ie => ie.BirimFiyat * ie.Adet)
                    }).ToList()
            }).ToList();

            ViewBag.AtolyeViewModel = atolyelerViewModel;

            return View(atolyeler);
        }















        // GET: Create (Yeni atölye ekleme formu)
        public IActionResult Create()
        {
            var viewModel = new AtolyeCreateEditViewModel
            {
                AvailableIsTipleri = _context.IsTipleri.ToList()
            };
            return View(viewModel);
        }

        // POST: Create (Yeni atölye ekleme işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AtolyeCreateEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var atolye = new Atolye
                {
                    Ad = viewModel.Ad,
                    Aciklama = viewModel.Aciklama,
                    AtolyeIsler = viewModel.SelectedIsTipleri.Select(isTipiId => new AtolyeIs
                    {
                        IsTipiId = isTipiId,
                        BirimFiyat = decimal.Parse(viewModel.BirimFiyatlar[isTipiId].ToString(), CultureInfo.InvariantCulture)
                    }).ToList()
                };

                _context.Atolyeler.Add(atolye);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            viewModel.AvailableIsTipleri = _context.IsTipleri.ToList();
            return View(viewModel);
        }

        // GET: Edit (Atölye düzenleme formu)
        public async Task<IActionResult> Edit(int id)
        {
            var atolye = await _context.Atolyeler
                .Include(a => a.AtolyeIsler)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atolye == null)
            {
                return NotFound();
            }

            var viewModel = new AtolyeCreateEditViewModel
            {
                Id = atolye.Id,
                Ad = atolye.Ad,
                Aciklama = atolye.Aciklama,
                AvailableIsTipleri = _context.IsTipleri.ToList(),
                SelectedIsTipleri = atolye.AtolyeIsler.Select(ai => ai.IsTipiId).ToList(),
                EskiBirimFiyatlar = atolye.AtolyeIsler.ToDictionary(ai => ai.IsTipiId, ai => ai.BirimFiyat)
            };

            return View(viewModel);
        }

        // POST: Edit (Atölye düzenleme işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AtolyeCreateEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var atolye = await _context.Atolyeler
                    .Include(a => a.AtolyeIsler)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atolye == null)
                {
                    return NotFound();
                }

                atolye.Ad = viewModel.Ad;
                atolye.Aciklama = viewModel.Aciklama;

                var existingIsTipleri = atolye.AtolyeIsler.ToList();

                // Remove deselected IsTipi
                foreach (var existing in existingIsTipleri)
                {
                    if (!viewModel.SelectedIsTipleri.Contains(existing.IsTipiId))
                    {
                        _context.AtolyeIsler.Remove(existing);
                    }
                }

                // Add or update selected IsTipi
                foreach (var isTipiId in viewModel.SelectedIsTipleri)
                {
                    var existingIs = atolye.AtolyeIsler.FirstOrDefault(ai => ai.IsTipiId == isTipiId);
                    if (existingIs != null)
                    {
                        existingIs.BirimFiyat = decimal.Parse(viewModel.BirimFiyatlar[isTipiId].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        atolye.AtolyeIsler.Add(new AtolyeIs
                        {
                            IsTipiId = isTipiId,
                            BirimFiyat = decimal.Parse(viewModel.BirimFiyatlar[isTipiId].ToString().Replace(",", "."), CultureInfo.InvariantCulture)
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.AvailableIsTipleri = _context.IsTipleri.ToList();
            return View(viewModel);
        }



        // POST: Delete (Atölye silme işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var atolye = await _context.Atolyeler.FindAsync(id);
            if (atolye == null)
            {
                return NotFound();
            }

            _context.Atolyeler.Remove(atolye);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
