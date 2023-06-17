using System.Collections.Generic;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.User.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<ShoppingCart> shoppingCart { get; set; }

        public decimal price { get; set; }

        public Coupon coupon { get; set; }

        public string message { get; set; }
    }
}
