using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FasonPortal.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }

        [Required]
        public string Role { get; set; }

        public int? FabrikaId { get; set; }
        public int? AtolyeId { get; set; }
        public int? SelectedFabrikaAtolye { get; set; }
    }


}

