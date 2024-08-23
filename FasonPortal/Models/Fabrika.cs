using FasonPortal.Data;

namespace FasonPortal.Models
{
    public class Fabrika

    {
        public int Id { get; set; }
        public string Ad { get; set; } // Fabrika adı
        public string Aciklama { get; set; } // Fabrika hakkında açıklama
        public List<IsEmri> IsEmirleri { get; set; } = new List<IsEmri>(); // Fabrikaya ait iş emirleri
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>(); // İlişkili kullanıcılar
    }

}
