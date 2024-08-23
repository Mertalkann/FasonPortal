using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FasonPortal.Data;
using FasonPortal.Models;
using FasonPortal.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Collections.Generic; // Liste için gerekli
using System.IO; // Dosya işlemleri için gerekli
using System.Threading; // Asenkron işlemler için gerekli

[Authorize(Roles = "FabrikaYetkilisi")]
public class FabrikaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConverter _converter;
    private readonly IWebHostEnvironment _env;
    private readonly IEmailSender _emailSender;  // EmailSender servisi

    public FabrikaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConverter converter, IWebHostEnvironment env, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _converter = converter;
        _env = env;
        _emailSender = emailSender;  // EmailSender servisini constructor'a ekleme
    }

    // Siparişlerim sayfası
    public async Task<IActionResult> Index()
    {
        // Giriş yapan kullanıcıyı alıyoruz
        var user = await _userManager.GetUserAsync(User);

        // Giriş yapan kullanıcının fabrikanın siparişlerini getiriyoruz
        var isEmirleri = _context.IsEmirleri
            .Where(e => e.FabrikaId == user.FabrikaId)
            .Select(e => new IsEmriViewModel
            {
                Id = e.Id, // Id'yi ViewModel'e ekliyoruz
                IsTipiId = e.IsTipiId,
                IsTipiAd = e.IsTipi.Ad,
                AtolyeId = e.AtolyeId,
                AtolyeAd = e.Atolye.Ad,
                Adet = e.Adet,
                BirimFiyat = e.BirimFiyat,
                Aciklama = e.Aciklama,
                Durum = e.Durum,
                OlusturulmaTarihi = e.OlusturulmaTarihi // Bu satırı ekleyin
            })
            .OrderByDescending(e => e.OlusturulmaTarihi)
            .ToList();

        return View(isEmirleri);
    }

    // Yeni sipariş oluşturma sayfası
    public IActionResult Create()
    {
        var isTipleri = _context.IsTipleri.ToList(); // İş tiplerini alıyoruz
        ViewBag.IsTipleri = new SelectList(isTipleri, "Id", "Ad");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IsEmriViewModel model)
    {
        ModelState.Remove("AtolyeAd");
        ModelState.Remove("IsTipiAd");
        ModelState.Remove("FabrikaAd");

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            var isEmri = new IsEmri
            {
                IsTipiId = model.IsTipiId,
                AtolyeId = model.AtolyeId,
                Adet = model.Adet,
                BirimFiyat = model.BirimFiyat,
                Aciklama = model.Aciklama,
                Durum = model.Durum,
                FabrikaId = user.FabrikaId.Value,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.IsEmirleri.Add(isEmri);
            await _context.SaveChangesAsync();

            // Sipariş oluşturulduktan sonra veritabanından gerekli bilgileri çekme
            var yeniIsEmri = _context.IsEmirleri
                .Include(e => e.IsTipi)
                .Include(e => e.Atolye)
                .Include(e => e.Fabrika)
                .FirstOrDefault(e => e.Id == isEmri.Id);

            if (yeniIsEmri != null)
            {
                model.FabrikaAd = yeniIsEmri.Fabrika.Ad;
                model.AtolyeAd = yeniIsEmri.Atolye.Ad;
                model.IsTipiAd = yeniIsEmri.IsTipi.Ad;
            }

            // Atölye kullanıcılarının e-posta adreslerini ve telefon numaralarını alma
            var atolyeler = _context.AtolyeIsler
                .Where(ai => ai.IsTipiId == model.IsTipiId && ai.AtolyeId == model.AtolyeId)
                .Select(ai => ai.Atolye)
                .ToList();

            foreach (var atolye in atolyeler)
            {
                var atolyeKullanicilari = _context.Users
                    .Where(u => u.AtolyeId == atolye.Id)
                    .ToList();

                foreach (var kullanici in atolyeKullanicilari)
                {
                    string subject = $"Yeni Sipariş Bilgisi - <strong>{model.FabrikaAd}</strong>";
                    string message = $@"
                <p>Merhaba <strong>{kullanici.FullName}</strong>,</p>

                <p>Sistemimizde {model.FabrikaAd} adına yeni bir sipariş oluşturulmuştur. Aşağıda siparişe ait detayları bulabilirsiniz:</p>

                <ul>
                    <li><strong>İş Tipi:</strong> {model.IsTipiAd}</li>
                    <li><strong>Adet:</strong> {model.Adet}</li>
                    <li><strong>Birim Fiyat:</strong> {model.BirimFiyat} TL</li>
                    <li><strong>Açıklama:</strong> {model.Aciklama}</li>
                </ul>

                <p>Bu sipariş, atölyeniz tarafından işleme alınmak üzere oluşturulmuştur. Siparişin detaylarını dikkatlice incelemenizi ve belirtilen şartlar doğrultusunda gerekli hazırlıkları yapmanızı rica ederiz. Siparişin teslim süreci, malzeme tedariği veya diğer operasyonel işlemlerle ilgili herhangi bir sorunuz olursa, lütfen aşağıdaki irtibat numarasından bizimle iletişime geçmekten çekinmeyin.</p>

                <p><strong>Telefon:</strong> {user.PhoneNumber}</p> <!-- Fabrika yetkilisinin telefon numarası -->
                
                <p>Atölyeniz için belirlenen iş tipleri ve diğer sipariş detayları hakkında daha fazla bilgi almak için, sipariş yönetim ekranımıza göz atabilir veya doğrudan atölye yönetim biriminizle iletişime geçebilirsiniz.</p>

                <p>İş birliğiniz için teşekkür ederiz. Her türlü sorunuzda yanınızdayız.</p>

                <p>Saygılarımızla,</p>
                <p>FasonPortal Ekibi</p>
                ";

                    // E-posta gönderme işlemi
                    await _emailSender.SendEmailAsync(new List<string> { kullanici.Email }, subject, message);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        var isTipleri = _context.IsTipleri.ToList();
        ViewBag.IsTipleri = new SelectList(isTipleri, "Id", "Ad");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var fabrikaId = user.FabrikaId;

        var isEmri = _context.IsEmirleri
            .Include(e => e.IsTipi)  // İş tipi bilgilerini yükleme
            .Include(e => e.Atolye)  // Atölye bilgilerini yükleme
            .Include(e => e.Fabrika) // Fabrika bilgilerini yükleme
            .FirstOrDefault(e => e.Id == id);

        if (isEmri != null && isEmri.FabrikaId == fabrikaId)
        {
            isEmri.Durum = "İptal Edildi";
            _context.Update(isEmri);
            await _context.SaveChangesAsync();

            // Atölye kullanıcılarına e-posta gönderimi
            var atolyeKullanicilari = _context.Users
                .Where(u => u.AtolyeId == isEmri.AtolyeId)
                .ToList();

            string subjectAtolye = $"Sipariş İptal Bilgisi - {isEmri.Fabrika.Ad}";

            foreach (var kullanici in atolyeKullanicilari)
            {
                string messageAtolye = $@"
            <p>Merhaba {kullanici.FullName},</p>

            <p><strong>{isEmri.Fabrika.Ad}</strong> adına daha önce oluşturulmuş olan bir sipariş iptal edilmiştir. Aşağıda iptal edilen siparişe ait detaylar yer almaktadır:</p>

            <ul>
                <li><strong>Sipariş ID:</strong> {isEmri.Id}</li> <!-- Sipariş ID'si eklendi -->
                <li><strong>İş Tipi:</strong> {isEmri.IsTipi.Ad}</li>
                <li><strong>Adet:</strong> {isEmri.Adet}</li>
                <li><strong>Birim Fiyat:</strong> {isEmri.BirimFiyat} TL</li>
                <li><strong>Açıklama:</strong> {isEmri.Aciklama}</li>
            </ul>

            <p>Bu siparişin iptal edilmesi nedeniyle atölyenizin ilgili birimleriyle iptal işlemi hakkında bilgi paylaşmanızı ve gerekli düzenlemeleri yapmanızı rica ederiz. Siparişin iptali ve diğer operasyonel işlemler hakkında herhangi bir sorunuz olursa, aşağıdaki irtibat numarasından bizimle iletişime geçmekten çekinmeyin.</p>

            <p><strong>Telefon:</strong> {user.PhoneNumber}</p> <!-- Fabrika yetkilisinin telefon numarası -->

            <p>Detaylar için sistemimizdeki sipariş yönetim ekranını ziyaret edebilir veya doğrudan atölye yönetim biriminizle iletişime geçebilirsiniz.</p>

            <p>İş birliğiniz için teşekkür ederiz. Her türlü sorunuzda yanınızdayız.</p>

            <p>Saygılarımızla,</p>
            <p>FasonPortal Ekibi</p>
            ";

                // E-posta gönderme işlemi
                await _emailSender.SendEmailAsync(new List<string> { kullanici.Email }, subjectAtolye, messageAtolye);
            }
        }
        else
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }






    // RaporAl metodu
    public async Task<IActionResult> RaporAl()
    {
        var user = await _userManager.GetUserAsync(User);

        // IsEmri ile ilişkili tüm detayları yüklüyoruz
        var isEmirleri = _context.IsEmirleri
            .Include(e => e.IsTipi)
            .Include(e => e.Atolye)
            .Include(e => e.Fabrika)
            .Where(e => e.FabrikaId == user.FabrikaId)
            .OrderByDescending(e => e.OlusturulmaTarihi)
            .ToList();

        // IsEmri modelini IsEmriViewModel'e dönüştürme
        var isEmriViewModels = isEmirleri.Select(e => new IsEmriViewModel
        {
            Id = e.Id,
            IsTipiId = e.IsTipiId,
            IsTipiAd = e.IsTipi?.Ad,
            AtolyeId = e.AtolyeId,
            AtolyeAd = e.Atolye?.Ad,
            Adet = e.Adet,
            BirimFiyat = e.BirimFiyat,
            Aciklama = e.Aciklama,
            Durum = e.Durum,
            OlusturulmaTarihi = e.OlusturulmaTarihi,
            FabrikaAd = e.Fabrika?.Ad
        }).ToList();

        var htmlContent = await this.RenderViewAsync("RaporTemplate", isEmriViewModels, true);

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
        return File(pdfBytes, "application/pdf", "SiparisRaporu.pdf");
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
            FabrikaId = isEmri.FabrikaId,
            Adet = isEmri.Adet,
            BirimFiyat = isEmri.BirimFiyat,
            Aciklama = isEmri.Aciklama,
            Durum = isEmri.Durum,
            IsTipiAd = isEmri.IsTipi.Ad,  // İş tipi adını doldurma
            AtolyeAd = isEmri.Atolye.Ad,   // Atölye adını doldurma
            FabrikaAd = isEmri.Fabrika.Ad  // Fabrika adını doldurma
        };

        ViewBag.IsTipleri = new SelectList(_context.IsTipleri, "Id", "Ad", model.IsTipiId);
        ViewBag.Atolyeler = new SelectList(_context.AtolyeIsler
            .Where(ai => ai.IsTipiId == model.IsTipiId)
            .Select(ai => ai.Atolye)
            .ToList(), "Id", "Ad", model.AtolyeId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(IsEmriViewModel model)
    {
        ModelState.Remove("AtolyeAd");
        ModelState.Remove("IsTipiAd");
        ModelState.Remove("FabrikaAd");

        if (ModelState.IsValid)
        {
            var isEmri = await _context.IsEmirleri
                .Include(e => e.IsTipi)
                .Include(e => e.Atolye)
                .Include(e => e.Fabrika)
                .FirstOrDefaultAsync(e => e.Id == model.Id);

            if (isEmri == null)
            {
                return NotFound();
            }

            // Sipariş bilgilerini güncelleme
            isEmri.Adet = model.Adet;
            isEmri.Aciklama = model.Aciklama;
            isEmri.BirimFiyat = model.BirimFiyat;

            _context.Update(isEmri);
            await _context.SaveChangesAsync();

            // Kullanıcı bilgilerini alma (siparişi oluşturan fabrika yetkilisi)
            var user = await _userManager.GetUserAsync(User);

            // E-posta gönderimi
            var atolyeler = _context.AtolyeIsler
                .Where(ai => ai.IsTipiId == model.IsTipiId && ai.AtolyeId == model.AtolyeId)
                .Select(ai => ai.Atolye)
                .ToList();

            foreach (var atolye in atolyeler)
            {
                var atolyeKullanicilari = _context.Users
                    .Where(u => u.AtolyeId == atolye.Id)
                    .ToList();

                foreach (var kullanici in atolyeKullanicilari)
                {
                    string subject = $"Sipariş Güncellemesi - {isEmri.Fabrika.Ad}";
                    string message = $@"
                <p>Merhaba {kullanici.FullName},</p>

                <p>{isEmri.Fabrika.Ad} adına daha önce oluşturulmuş olan bir sipariş üzerinde güncellemeler yapılmıştır. Aşağıda güncellenmiş siparişe ait detayları bulabilirsiniz:</p>

                <ul>
                    <li><strong>Sipariş ID:</strong> {isEmri.Id}</li>
                    <li><strong>Atölye:</strong> {isEmri.Atolye.Ad}</li>
                    <li><strong>İş Tipi:</strong> {isEmri.IsTipi.Ad}</li>
                    <li><strong>Güncellenen Adet:</strong> {isEmri.Adet}</li>
                    <li><strong>Birim Fiyat:</strong> {isEmri.BirimFiyat} TL</li>
                    <li><strong>Açıklama:</strong> {isEmri.Aciklama}</li>
                </ul>

                <p>Bu güncellemeler doğrultusunda, lütfen atölyenizin ilgili birimleriyle iletişime geçerek siparişin yeni detaylarına göre gerekli hazırlıkları yapınız. Siparişin teslim süreci ve diğer operasyonel işlemler hakkında herhangi bir sorunuz olursa, aşağıdaki irtibat numarasından bizimle iletişime geçmekten çekinmeyin.</p>

                <p><strong>FabrikaTelefon:</strong> {user.PhoneNumber}</p>

                <p>Güncellenmiş sipariş durumu ve diğer detaylar için sistemimizdeki sipariş yönetim ekranını ziyaret edebilir veya doğrudan atölye yönetim biriminizle irtibata geçebilirsiniz.</p>

                <p>İş birliğiniz için teşekkür ederiz. Her türlü sorunuzda yanınızdayız.</p>

                <p>Saygılarımızla,</p>
                <p>FasonPortal Ekibi</p>
                ";

                    // E-posta gönderme işlemi
                    await _emailSender.SendEmailAsync(new List<string> { kullanici.Email }, subject, message);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Hatalı durumda iş tiplerini tekrar gönderiyoruz
        var isTipleri = _context.IsTipleri.ToList();
        ViewBag.IsTipleri = new SelectList(isTipleri, "Id", "Ad", model.IsTipiId);
        ViewBag.Atolyeler = new SelectList(_context.AtolyeIsler
            .Where(ai => ai.IsTipiId == model.IsTipiId)
            .Select(ai => ai.Atolye)
            .ToList(), "Id", "Ad", model.AtolyeId);

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
