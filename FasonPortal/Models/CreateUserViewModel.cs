using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FasonPortal.Models
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }

        [Required]
        public string Role { get; set; }

        public int? FabrikaId { get; set; }
        public int? AtolyeId { get; set; }

        // Fabrika ve Atölye listeleri için kullanılacak SelectList
        
        public int? SelectedFabrikaAtolye { get; set; }
    }
}
