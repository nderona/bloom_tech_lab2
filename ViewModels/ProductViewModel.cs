using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPetal_Lab.ViewModels
{
    public class ProductViewModel
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


        public Guid SelectedCategory { get; set; }

        public SelectListItem Categories { get; set; }


        public IFormFile DefaultPhoto { get; set; }

        public List<IFormFile> Images { get; set; }
    }
}
