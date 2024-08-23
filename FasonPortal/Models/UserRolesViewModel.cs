using System.Collections.Generic;

namespace FasonPortal.Models
{
    public class UserRolesViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
        public string FabrikaAd { get; set; } // Fabrika Adı
        public string AtolyeAd { get; set; } // Atölye Adı


    }
}
