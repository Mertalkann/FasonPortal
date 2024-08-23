namespace FasonPortal.Models
{
    public class IsTipi
    {
        public int Id { get; set; }
        public string Ad { get; set; }  // İşin adı (Örneğin, "Bileklik Üretimi")
        public List<AtolyeIs> AtolyeIsler { get; set; } // Atölye ile ilişki
        public List<IsEmri> IsEmirleri { get; set; } // Bu iş tipine ait iş emirleri
    }
}
