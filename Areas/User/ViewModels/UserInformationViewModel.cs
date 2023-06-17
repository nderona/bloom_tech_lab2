using System;
using System.ComponentModel.DataAnnotations;

namespace TechPetal_Lab.Areas.User.ViewModels
{
    public class UserInformationViewModel
    {
        [Required]
        public string City { get; set; }
        [Required]
        public string Address1 { get; set; }
        [Required]
        public string Address2 { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public Guid ProductId { get; set; }

        public Guid CouponId { get; set; }
    }
}
