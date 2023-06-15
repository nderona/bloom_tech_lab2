using System;
using System.ComponentModel.DataAnnotations.Schema;
using TechPetal_Lab.Models;
namespace TechPetal_Lab.ViewModels
{
    public class HomeDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }

        [Column( TypeName = "decimal(18,2)" )]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }
        public string Brand { get; set; }
        public Category Category { get; set; }
        public byte[] DefaultImage { get; set; }
    }
}
