using System;

namespace TechPetal_Lab.Models
{
    public class Coupon
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public int Discount { get; set; }

        public bool Public { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpireDate { get; set; }

    }
}
