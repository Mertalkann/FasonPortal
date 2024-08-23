namespace FasonPortal.Models
{
    public class AtolyeRaporViewModel
    {
        public string FabrikaAd { get; set; } // Fabrika adı
        public string AtolyeAd { get; set; } // Atölyenin adı
        public List<FabrikaBazliSiparisViewModel> FabrikaGruplari { get; set; }
        public List<IsEmriViewModel> IsEmirleri { get; set; } // Siparişlerin listesi
    }
}
