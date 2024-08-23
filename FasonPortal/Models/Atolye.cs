using FasonPortal.Data;

namespace FasonPortal.Models
{
    public class Atolye
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Aciklama { get; set; }
        public List<AtolyeIs> AtolyeIsler { get; set; } // Atölyenin yapabileceği işler
        public List<IsEmri> IsEmirleri { get; set; } // Atölyeye ait iş emirleri
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();  // Kullanıcılar ilişkisi
    }
}
