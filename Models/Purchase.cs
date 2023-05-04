using BloomTech.Models;
using System;
using BloomTech.Enums;

namespace BloomTech.Models
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid WebUserId { get; set; }
        public WebUser WebUser { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public int Quantity { get; set; }
        public string PhoneNumber { get; set; }
        public bool Received { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
