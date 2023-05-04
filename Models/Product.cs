using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloomTech.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        [Column( TypeName = "decimal(18,2)" )]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Sale { get; set; }
        public int Bought { get; set; }
        public DateTime AddedDate { get; set; }
        public string Brand { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public byte[] DefaultImage { get; set; }

    }
}
