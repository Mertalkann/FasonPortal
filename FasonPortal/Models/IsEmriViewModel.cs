using System.ComponentModel.DataAnnotations;

namespace FasonPortal.Models
{
    public class IsEmriViewModel
    {
        public int Id { get; set; } // Sipariş ID'si

        [Required]
        public int IsTipiId { get; set; }
        public string IsTipiAd { get; set; }

        [Required]
        public int AtolyeId { get; set; }
        public string AtolyeAd { get; set; }
        public int FabrikaId { get; set; }
        public string FabrikaAd { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Adet 1'den büyük olmalıdır.")]
        public int Adet { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0.01'den büyük olmalıdır.")]
        public decimal BirimFiyat { get; set; }

        public string Aciklama { get; set; }
        public string Durum { get; set; } = "Beklemede";
        public DateTime OlusturulmaTarihi { get; set; }
       

    }
}
