using System;

namespace TechPetal_Lab.ViewModels
{
    public class PurchaseHistoryProduct
    {
        public string Name { get; set; }
        public byte[] DefaultImage { get; set; }
        public bool Reviewed { get; set; }
        public Guid ReviewedId { get; set; }
        public Guid ProductId { get; set; }
        public int AmountBought { get; set; }
    }
}
