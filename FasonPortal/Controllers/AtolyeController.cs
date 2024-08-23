using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FasonPortal.Data;
using System.Linq;
using FasonPortal.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

public class AtolyeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AtolyeController> _logger;

    public AtolyeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<AtolyeController> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var fabrikaBazliSiparisler = _context.IsEmirleri
            .Where(e => e.Atolye.Users.Any(u => u.Id == user.Id))
            .Include(e => e.IsTipi)
            .Include(e => e.Fabrika)
            .GroupBy(e => e.Fabrika.Ad)
            .Select(g => new FabrikaBazliSiparisViewModel
            {
                FabrikaAd = g.Key,
                Siparisler = g.Select(s => new IsEmriViewModel
                {
                    Id = s.Id,
                    IsTipiAd = s.IsTipi.Ad,
                    Adet = s.Adet,
                    BirimFiyat = s.BirimFiyat,
                    Aciklama = s.Aciklama,
                    Durum = s.Durum,
                    OlusturulmaTarihi = s.OlusturulmaTarihi
                }).ToList()
            }).ToList();

        return View(fabrikaBazliSiparisler);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, string newStatus)
    {
        try
        {
            var siparis = await _context.IsEmirleri
                .Include(e => e.Fabrika)
                .Include(e => e.Atolye)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (siparis == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            // 'Onayla' tıklaması durumunda durumu 'Onaylandı' olarak ayarla
            if (newStatus == "Onaylandı")
            {
                newStatus = "Onaylandı";
            }
            else if (newStatus == "Geri Al")
            {
                newStatus = "Beklemede";
            }

            var previousStatus = siparis.Durum;
            siparis.Durum = newStatus;

            _context.Update(siparis);

            var saveResult = await _context.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return Json(new { success = false, message = "Veritabanına kayıt işlemi başarısız oldu." });
            }

            var fabrikaYetkilisi = await _context.Users.FirstOrDefaultAsync(u => u.FabrikaId == siparis.FabrikaId);
            if (fabrikaYetkilisi == null)
            {
                _logger.LogError("Fabrika yetkilisi bulunamadı. Fabrika ID: {FabrikaId}", siparis.FabrikaId);
                return Json(new { success = false, message = "Fabrika yetkilisi bulunamadı." });
            }

            var atolyeYetkilisi = await _context.Users.FirstOrDefaultAsync(u => u.AtolyeId == siparis.AtolyeId);
            if (atolyeYetkilisi == null)
            {
                _logger.LogError("Atölye yetkilisi bulunamadı. Atölye ID: {AtolyeId}", siparis.AtolyeId);
                return Json(new { success = false, message = "Atölye yetkilisi bulunamadı." });
            }

            // E-posta gönderimi
            try
            {
                string subject = $"Sipariş Durum Güncellemesi: Sipariş {siparis.Id} - {siparis.Atolye?.Ad ?? "Atölye Bilgisi Yok"}";
                string message = $@"
            <p>Sayın {fabrikaYetkilisi.FullName},</p>
            <p>{siparis.Id} numaralı siparişinizin durumu atölyemiz tarafından güncellenmiştir. Aşağıda güncellenen siparişe ait detayları bulabilirsiniz:</p>
            <ul>
                <li><strong>Sipariş ID:</strong> {siparis.Id}</li>
                <li><strong>Fabrika:</strong> {siparis.Fabrika?.Ad ?? "Fabrika Bilgisi Yok"}</li>
                <li><strong>Atölye:</strong> {siparis.Atolye?.Ad ?? "Atölye Bilgisi Yok"}</li>
                <li><strong>İş Tipi:</strong> {siparis.IsTipi?.Ad ?? "İş Tipi Bilgisi Yok"}</li>
                <li><strong>Adet:</strong> {siparis.Adet}</li>
                <li><strong>Birim Fiyat:</strong> {siparis.BirimFiyat} TL</li>
                <li><strong>Açıklama:</strong> {siparis.Aciklama}</li>
                <li><strong>Yeni Durum:</strong> {newStatus}</li>
            </ul>";

                message += $@"
            </p>
            <p>Herhangi bir sorunuz olması durumunda, aşağıdaki irtibat numarasından bizimle iletişime geçebilirsiniz:</p>
            <p><strong>Telefon:</strong> {atolyeYetkilisi?.PhoneNumber ?? "Belirtilmedi"}</p>
            <p>Detaylı bilgi ve sipariş durumunu görmek için sistemimizdeki sipariş yönetim ekranını ziyaret edebilirsiniz.</p>
            <p>Saygılarımızla,</p>
            <p>{siparis.Atolye?.Ad ?? "Atölye Bilgisi Yok"} Atölyesi</p>";

                await _emailSender.SendEmailAsync(new List<string> { fabrikaYetkilisi.Email }, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError("E-posta gönderimi sırasında bir hata oluştu: {Message}", ex.Message);
                return Json(new { success = false, message = "E-posta gönderimi sırasında bir hata oluştu.", error = ex.Message });
            }

            return Json(new { success = true, previousStatus = previousStatus, newStatus = newStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError("Durum değişikliği sırasında bir hata oluştu: {Message}", ex.Message);
            return Json(new { success = false, message = "Durum değişikliği sırasında bir hata oluştu.", error = ex.Message });
        }
    }




    public async Task<IActionResult> GetUserRoles()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
        {
            // Admin rolü için özel işlemler
        }

        return View();
    }

  
}
