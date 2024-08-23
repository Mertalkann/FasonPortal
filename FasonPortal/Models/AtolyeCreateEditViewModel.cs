using FasonPortal.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FasonPortal.Models
{
    public class AtolyeCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; }

        public string Aciklama { get; set; }

        public List<IsTipi> AvailableIsTipleri { get; set; } = new List<IsTipi>();

        [Required(ErrorMessage = "En az bir iş tipi seçmelisiniz.")]
        public List<int> SelectedIsTipleri { get; set; } = new List<int>();

        public Dictionary<int, decimal> BirimFiyatlar { get; set; } = new Dictionary<int, decimal>();

        public Dictionary<int, decimal> EskiBirimFiyatlar { get; set; } = new Dictionary<int, decimal>();
        public List<IsEmriViewModel> IsEmirleri { get; set; } = new List<IsEmriViewModel>();
        public int IsTipiAdedi => SelectedIsTipleri?.Distinct().Count() ?? 0; // İş Tipi Sayısı
        public int SiparisAdedi => IsEmirleri?.Count() ?? 0; // Toplam Sipariş Sayısı    }
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();  // Kullanıcılar ilişkisi
    }
}