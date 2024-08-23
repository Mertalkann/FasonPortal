namespace FasonPortal.Models
{
    public class FabrikaRaporViewModel
    {
        public string FabrikaAd { get; set; } // Fabrikanın adı
        public List<IsEmriViewModel> IsEmirleri { get; set; } // Siparişlerin listesi
    }
}
