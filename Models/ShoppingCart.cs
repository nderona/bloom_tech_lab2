using System;

namespace TechPetal_Lab.Models
{
    public class ShoppingCart
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public Guid WebUserId { get; set; }
        public WebUser WebUser { get; set; }

        public Guid? UserCouponId { get; set; }
        public UserCoupon UserCoupon { get; set; }

        public Guid? CouponId { get; set; }

        public Coupon Coupon { get; set; }

        public bool CouponApplied { get; set; }

    }
}
