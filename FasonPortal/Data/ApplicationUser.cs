using FasonPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace FasonPortal.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public int? FabrikaId { get; set; }
        public Fabrika Fabrika { get; set; }

        public int? AtolyeId { get; set; }
        public Atolye Atolye { get; set; }

        public ICollection<IdentityUserRole<string>> Roles { get; set; }
    }
}