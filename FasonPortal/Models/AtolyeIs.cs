namespace FasonPortal.Models
{
    /// <summary>
    /// Bir atölyenin birden fazla işi olabilr, bir işi birden fazla atölye yapıyor olabilir. N-N ilişkili ara tablo.
    /// </summary>
    public class AtolyeIs
    {
        public int Id { get; set; }
        public int AtolyeId { get; set; }
        public Atolye Atolye { get; set; } // Atölye ile ilişki
        public int IsTipiId { get; set; }
        public IsTipi IsTipi { get; set; } // İş tipi ile ilişki
        public decimal BirimFiyat { get; set; } // Bu işin birim fiyatı
    }
}
