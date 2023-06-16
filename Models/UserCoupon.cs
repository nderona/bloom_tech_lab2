using System;

namespace BloomTech.Models
{
    public class UserCoupon
    {
        public Guid Id { get; set; }
        public Guid WebUserId { get; set; }
        public WebUser WebUser { get; set; }
        public bool Used { get; set; }
        public Guid CouponId { get; set; }
        public Coupon Coupon { get; set; }
    }
}
