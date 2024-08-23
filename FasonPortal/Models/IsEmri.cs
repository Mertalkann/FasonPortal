using FasonPortal.Data;

namespace FasonPortal.Models
{
    public class IsEmri
    {
        public int Id { get; set; }

        public int IsTipiId { get; set; }
        public IsTipi IsTipi { get; set; } // İş tipi ile ilişki

        public int AtolyeId { get; set; }
        public Atolye Atolye { get; set; } // Atölye ile ilişki

        public int Adet { get; set; } // Sipariş edilen adet miktarı
        public decimal BirimFiyat { get; set; } // Birim fiyat
        public string Aciklama { get; set; } // Sipariş ile ilgili açıklamalar

        public string Durum { get; set; } // Siparişin durumu
        public DateTime OlusturulmaTarihi { get; set; } // Siparişin oluşturulma tarihi

        public int FabrikaId { get; set; }
        public Fabrika Fabrika { get; set; } // Fabrika ile ilişki
    }
}
